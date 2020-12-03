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
    let initialize eventStore =

        let handlePostWith booking = fun next context ->
            task {
                let cmd =
                    {
                        BookADesk.OfficeId = Guid.Parse(booking.OfficeId) |> OfficeId // Consider TryParse and return 400 if not valid
                        Date = booking.BookingDate
                        EmailAddress = EmailAddress booking.EmailAddress
                    }

                let command = BookADesk cmd
                let commandHandler = BookADeskCommandHandler.provide eventStore

                let result = commandHandler.Handle command
                match result with
                | Ok _ -> return! json cmd next context
                | Error e -> return! failwith e
            }

        {
            HandlePostWith = handlePostWith
        }
