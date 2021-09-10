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
    let initialize
        (provideEventStore : IAmazonDynamoDB -> DynamoDbEventStore)
        reservationCommandsFactory
        (notifySuccess: Models.Booking-> Async<bool>)
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
                
                let handleCommand command = async {
                    let (ReservationId aggregateId) = ReservationAggregate.Id

                    match! eventStore.GetEvents aggregateId with
                    | Ok events ->
                        let handler = ReservationsCommandHandler.provide (events |> List.ofSeq) reservationCommandsFactory
                        let results = handler.Handle command
                                        |> Result.mapError errorHandler.MapReservationErrorToAssignBookADeskError

                        match results with
                        | Ok events ->
                            let appendEvents eventsToAppend : Result<Async<unit>,ResponseError> =
                                eventsToAppend
                                |> Seq.ofList
                                |> (fun events -> aggregateId, events)
                                |> List.singleton
                                |> Map.ofList
                                |> eventStore.AppendEvents
                                |> Ok

                            return appendEvents events
                        | Error error ->
                            return error
                                    |> errorHandler.ConvertErrorToResponseError
                                    |> Error
                    | Error error ->
                        return (errorHandler.MapStringToAssignBookADeskError error)
                                |> errorHandler.ConvertErrorToResponseError
                                |> Error
                }

                let! result = handleCommand command
                match result with
                | Ok _ ->
                    let output =
                        {
                            Office = { Id = booking.Office.Id }
                            Date = booking.Date
                            User = { Email = booking.User.Email }
                        }
                    let! sent = notifySuccess booking
                    match sent with
                        | true -> printfn $"Notification message sent for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()}"
                        | false -> failwithf $"Error sending notification error for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()}"
                        
                    return! json output next context
                | Error (response: ResponseError) ->
                    context.SetStatusCode(response.StatusCode)
                    return! json response.Error next context
            }

        {
            HandlePostWith = handlePostWith
        }
