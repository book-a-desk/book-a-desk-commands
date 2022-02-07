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

type CancelBookingsHttpHandler =
    {
        HandlePostWith: Models.Cancellation -> HttpHandler
    }

module CancelBookingsHttpHandler =
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
        (errorHandler: BookADeskErrorHandler) =

        let handlePostWith (cancelBooking:Cancellation) = fun next (context : HttpContext) ->
            task {
                let cmd =
                    {
                        CancelBookADesk.OfficeId = Guid.Parse(cancelBooking.Office.Id) |> OfficeId // Consider TryParse and return 400 if not valid
                        Date = cancelBooking.Date
                        EmailAddress = EmailAddress cancelBooking.User.Email
                    }

                let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())
                let command = CancelBookADesk cmd

                let! result = handleCommand command eventStore reservationCommandsFactory errorHandler
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