namespace Book_A_Desk.Api

open System

open System.Net.Mail
open Amazon.DynamoDBv2
open Microsoft.AspNetCore.Http
open Giraffe
open FsToolkit.ErrorHandling
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.QueriesHandler
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Domain

open Book_A_Desk.Infrastructure.DynamoDbEventStore

type BookingsHttpHandler =
    {
        HandlePostWith: Models.Booking -> HttpHandler
    }

type EmailDetails =
    {
        toAddress : string
        fromAddress : string
        body : string
    }

type MessageDetails =
    | Email of EmailDetails

module BookingsHttpHandler =
    let initialize (provideEventStore : IAmazonDynamoDB -> DynamoDbEventStore) reservationCommandsFactory sendEmailNotification =
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
                
                let handleCommand command = asyncResult {
                    let (ReservationId aggregateId) = ReservationAggregate.Id
                    let! events = eventStore.GetEvents aggregateId
                    
                    let handler = ReservationsCommandHandler.provide (events |> List.ofSeq) reservationCommandsFactory
                    let results = handler.Handle command
                    
                    match results with
                    | Ok events ->
                        let appendEvents eventsToAppend : Async<unit> = 
                            events
                            |> Seq.ofList
                            |> (fun events -> aggregateId, events)
                            |> List.singleton
                            |> Map.ofList
                            |> eventStore.AppendEvents
                            
                        return! appendEvents events
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
                    sendEmailNotification booking
                    |> ignore
                    
                    return! json output next context
                | Error e ->
                    context.SetStatusCode(500)
                    return! text ("Internal Error: " + e) next context
            }

        {
            HandlePostWith = handlePostWith
        }
