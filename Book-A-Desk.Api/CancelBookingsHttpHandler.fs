namespace Book_A_Desk.Api

open System

open Amazon.DynamoDBv2
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open FsToolkit.ErrorHandling.Operator.Result
open Microsoft.AspNetCore.Http
open Giraffe
open FsToolkit.ErrorHandling
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands

open Book_A_Desk.Infrastructure.DynamoDbEventStore

type CancelBookingsHttpHandler =
    {
        HandlePostWith: Models.Cancellation -> HttpHandler
    }

module CancelBookingsHttpHandler =
    let private handleCommand (command: CancelBookADesk) eventStore reservationCommandsFactory errorHandler = asyncResult {
        let aggregateId = command.AggregateId

        let! reservationAggregate =
            eventStore.GetReservationAggregateById aggregateId
            |> Async.map(
                Result.mapError (
                    errorHandler.MapStringToAssignBookADeskError >> errorHandler.ConvertErrorToResponseError))

        let handler =
            ReservationsCommandHandler.provide
                reservationAggregate
                reservationCommandsFactory
        
        let! events =
            handler.Handle (CancelBookADesk command)
            |> Result.mapError (
                errorHandler.MapReservationErrorToAssignBookADeskError >> errorHandler.ConvertErrorToResponseError)
                        
        let appendEventsToReservationAggregate (reservationAggregate: ReservationAggregate) (eventsToAppend: DomainEvent list) =
            {
                ReservationAggregate.Id = reservationAggregate.Id
                ReservationEvents =
                    eventsToAppend
                    |> List.map(fun (ReservationEvent reservationEvent) -> reservationEvent)
                    |> List.append reservationAggregate.ReservationEvents
            }
            |> eventStore.SaveReservationAggregate

        return! appendEventsToReservationAggregate reservationAggregate events
    }

    let initialize
        (provideEventStore : IAmazonDynamoDB -> DynamoDbEventStore)
        reservationCommandsFactory
        (errorHandler: BookADeskErrorHandler) =

        let handlePostWith (cancelBooking:Cancellation) = fun next (context : HttpContext) ->
            task {
                let cmd =
                    {
                        CancelBookADesk.AggregateId = cancelBooking.AggregateId
                        OfficeId = Guid.Parse(cancelBooking.Office.Id) |> OfficeId // Consider TryParse and return 400 if not valid
                        Date = cancelBooking.Date
                        EmailAddress = EmailAddress cancelBooking.User.Email
                    }

                let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())

                let! result = handleCommand cmd eventStore reservationCommandsFactory errorHandler
                match result with
                | Ok _ ->
                    let output =
                        "Booking have been cancelled"
                    
                    return! json output next context
                | Error (response: ResponseError) ->
                    context.SetStatusCode(response.StatusCode)
                    return! json response.Error next context
            }

        {
            HandlePostWith = handlePostWith
        }