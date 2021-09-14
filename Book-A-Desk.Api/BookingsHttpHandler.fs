namespace Book_A_Desk.Api

open System

open Amazon.DynamoDBv2
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
    }

module BookingsHttpHandler =
    let private notifyBooking output booking notifySuccess errorHandler next context =
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
        let (ReservationId aggregateId) = ReservationAggregate.Id
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
        (errorHandler: BookADeskErrorHandler) =

        let handlePostWith booking = fun next (context : HttpContext) ->
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
                            Office = { Id = booking.Office.Id }
                            Date = booking.Date
                            User = { Email = booking.User.Email }
                        }
                    
                    return! notifyBooking output booking notifySuccess errorHandler next context
                | Error (response: ResponseError) ->
                    context.SetStatusCode(response.StatusCode)
                    return! json response.Error next context
            }

        {
            HandlePostWith = handlePostWith
        }
