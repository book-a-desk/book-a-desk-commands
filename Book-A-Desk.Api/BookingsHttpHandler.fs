namespace Book_A_Desk.Api

open Amazon.DynamoDBv2
open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive
open System

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
        HandlePostWith: Booking -> HttpHandler
    }

module BookingsHttpHandler =
    let initialize (provideEventStore : IAmazonDynamoDB -> DynamoDbEventStore) reservationCommandsFactory =
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
                    let (ReservationId eventId) = ReservationAggregate.Id
                    let! events = eventStore.GetEvents eventId
                    
                    let handler = ReservationsCommandHandler.provide events reservationCommandsFactory
                    let results = handler.Handle command
                    
                    match results with
                    | Ok events ->
                        do!
                            events
                            |> List.singleton
                            |> Map.ofList
                            |> eventStore.AppendEvents
                    | Error _ -> return ()
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
                    return! json output next context
                | Error e ->
                    context.SetStatusCode(500)
                    return! text ("Internal Error: " + e) next context
            }

        {
            HandlePostWith = handlePostWith
        }
