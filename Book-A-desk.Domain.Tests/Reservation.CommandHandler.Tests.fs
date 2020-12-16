module Book_A_desk.Domain.Tests.ReservationCommandHandler

open System

open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands

let eventStore = InMemoryEventStore.provide ()
let reservationCommandsFactory = ReservationCommandsFactory.provide ()

[<Fact>]
let ``Handle xxxx test`` () =
    let reservationCommandHandler = BookADeskCommandHandler.provide eventStore reservationCommandsFactory

    let bookADesk =
        {
            BookADesk.EmailAddress = EmailAddress ""
            BookADesk.Date = DateTime.Now
            BookADesk.OfficeId = Offices.All.[0].Id
        }
        |> BookADesk

    let result = reservationCommandHandler.Handle bookADesk

    // ensure event store GetAllEvents is called

    // ensure event store StoreEvents is called

    // ensure command executor.ExecuteWith is called

    ()
