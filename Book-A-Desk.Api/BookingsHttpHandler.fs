namespace Book_A_Desk.Api

open System

open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Api.Models
type BookingsHttpHandler =
    {
        HandlePostWith: Booking -> HttpHandler
    }

module BookingsHttpHandler =
    let initialize eventStore getOffices =

        let handlePostWith booking = fun next context ->
            task {
                let cmd =
                    {
                        BookADesk.OfficeId = Guid.Parse(booking.Office.Id) |> OfficeId // Consider TryParse and return 400 if not valid
                        Date = booking.Date
                        EmailAddress = EmailAddress booking.User.Email
                    }
 
                let eventStore = eventStore (context.GetService<IAmazonDynamoDB>())
                let command = BookADesk cmd
                let commandHandler = BookADeskCommandHandler.provide eventStore getOffices

                let result = commandHandler.Handle command
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
