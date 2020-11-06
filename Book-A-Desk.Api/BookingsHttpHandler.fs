﻿namespace Book_A_Desk.Api

open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Api.Models
open Book_A_Desk.Commands.Domain

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
                        BookADesk.OfficeID = OfficeID booking.OfficeId
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
