namespace Book_A_Desk.Api

open System

open Amazon.DynamoDBv2
open Book_A_Desk.Domain.Reservation.Queries
open FsToolkit.ErrorHandling.Operator.Result
open Microsoft.AspNetCore.Http
open Giraffe
open FsToolkit.ErrorHandling
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Domain

open Book_A_Desk.Infrastructure.DynamoDbEventStore

type BookingsHttpHandler =
    {
        HandlePostWith: Models.Booking -> HttpHandler
        HandleGetByEmailAndDate: unit -> HttpHandler
    }

module BookingsHttpHandler =
    let private notifyBooking output (booking : Models.Booking) notifySuccess errorHandler next context =
        task {
            let! sent = notifySuccess booking
            match sent with
            | Ok _ ->
                printfn $"Notification message sent for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()}"
                return! json output next context
            | Error e ->
                let error =
                    $"Error sending notification error for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()} because {e}"
                let responseError = (errorHandler.MapStringToAssignBookADeskError error)
                                    |> errorHandler.ConvertErrorToResponseError
                context.SetStatusCode(StatusCodes.Status500InternalServerError)
                return! json responseError.Error next context
        }

    let private handleCommand command eventStore reservationCommandsFactory errorHandler = asyncResult {
        let! events =
            eventStore.GetEvents aggregateId
            |> Async.map(
                Result.mapError (
                    errorHandler.MapStringToAssignBookADeskError >> errorHandler.ConvertErrorToResponseError))
        let handler = ReservationsCommandHandler.provide (events |> List.ofSeq) reservationCommandsFactory
        
        let! events =
            handler.Handle command
            |> Result.mapError (
                errorHandler.MapReservationErrorToAssignBookADeskError >> errorHandler.ConvertErrorToResponseError)
                        
        let appendEvents eventsToAppend : Async<unit> =
            eventsToAppend
            |> Seq.ofList
            |> (fun events -> aggregateId, events)
            |> List.singleton
            |> Map.ofList
            |> eventStore.AppendEvents
            
        return! appendEvents events
    }

    let initialize
        (provideEventStore : IAmazonDynamoDB -> DynamoDbEventStore)
        reservationCommandsFactory
        (notifySuccess: Models.Booking-> Async<Result<unit, string>>)
        (featureFlags : FeatureFlags)
        (errorHandler: BookADeskErrorHandler) =

        let handlePostWith (booking : Models.Booking) = fun next (context : HttpContext) ->
            task {
                let cmd =
                    {
                        BookADesk.OfficeId = Guid.Parse(booking.Office.Id) |> OfficeId // Consider TryParse and return 400 if not valid
                        Date = booking.Date
                        EmailAddress = EmailAddress booking.User.Email
                    }

                let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())
                let command = BookADesk cmd

                let! result = handleCommand command eventStore reservationCommandsFactory errorHandler
                match result with
                | Ok _ ->
                    let output =
                        {
                            Booking.Office = { Id = booking.Office.Id }
                            Date = booking.Date
                            User = { Email = booking.User.Email }
                        }
                    
                    return! notifyBooking output booking notifySuccess errorHandler next context
                | Error (response: ResponseError) ->
                    context.SetStatusCode(response.StatusCode)
                    return! json response.Error next context
            }

        let handleGetByEmailAndDateFromEventStore eventStore email date = asyncResult {
            let! bookingEvents = eventStore.GetEvents aggregateId
            return! ReservationsQueriesHandler.getUserBookingsStartFrom bookingEvents email date
        }
        
        let handleGetByDateFromEventStore eventStore date = asyncResult {
            let! bookingEvents = eventStore.GetEvents aggregateId
            return! ReservationsQueriesHandler.getUsersBookingsStartFrom bookingEvents date
        }
        
        let handleGetByEmailAndDate () = fun next context ->
            task {
                if featureFlags.GetBookings then
                    let email = InputParser.parseEmailFromContext context
                    let date = InputParser.parseDateFromContext context
                    match email, date with
                    | None, None ->
                        context.SetStatusCode(400)
                        return! text "Email and start date could not be parsed" next context
                    | Some _, None ->
                        context.SetStatusCode(400)
                        return! text "Start date could not be parsed" next context
                    | None, Some date ->
                        let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())
                        let! result = handleGetByDateFromEventStore eventStore date
                        
                        match result with
                        | Ok bookings ->
                            let bookings =
                                bookings
                                |> List.map (fun (booking:Booking) ->
                                    Booking.value booking.OfficeId booking.Date booking.EmailAddress
                                    )
                                |> List.toArray
                                |> fun l -> { Bookings.Items = l }
                            return! json bookings next context
                        | Error e ->
                            context.SetStatusCode(500)
                            return! text ("Internal Error: " + e) next context
                    | Some email, Some date ->
                        let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())
                        let! result = handleGetByEmailAndDateFromEventStore eventStore email date
                        
                        match result with
                        | Ok bookings ->
                            let bookings =
                                bookings
                                |> List.map (fun (booking:Booking) ->
                                    Booking.value booking.OfficeId booking.Date booking.EmailAddress
                                    )
                                |> List.toArray
                                |> fun l -> { Bookings.Items = l }
                            return! json bookings next context
                        | Error e ->
                            context.SetStatusCode(500)
                            return! text ("Internal Error: " + e) next context
                else
                    context.SetStatusCode(404)
                    return! text "Not Found" next context
        }
        
        {
            HandlePostWith = handlePostWith
            HandleGetByEmailAndDate = handleGetByEmailAndDate
        }
