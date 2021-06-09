namespace Book_A_Desk.Api

open System

open System.Net.Mail
open Book_A_Desk.Domain.QueriesHandler
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

type EmailDetails =
    {
        toAddress : string
        fromAddress : string
        body : string
    }

type MessageDetails =
    | Email of EmailDetails

module BookingsHttpHandler =
    let initialize eventStore reservationCommandsFactory sendEmailNotification =        
        let handlePostWith booking = fun next context ->
            task {
                let cmd =
                    {
                        BookADesk.OfficeId = Guid.Parse(booking.Office.Id) |> OfficeId // Consider TryParse and return 400 if not valid
                        Date = booking.Date
                        EmailAddress = EmailAddress booking.User.Email
                    }

                let command = BookADesk cmd
                let commandHandler = ReservationsCommandHandler.provide eventStore reservationCommandsFactory

                let result = commandHandler.Handle command
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
