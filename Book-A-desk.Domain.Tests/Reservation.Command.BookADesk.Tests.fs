module Book_A_Desk.Domain.Reservation.Commands.Tests

open System
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let commandIsValid _ _ = Ok ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command and desks are available, THEN an event should be created`` () =
    let command = BookADeskReservationCommand.provide commandIsValid

    let office = Offices.All.[0]
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeId = office.Id
    }

    let result = command.ExecuteWith bookADesk ReservationAggregate.Create
    match result with
    | Error _ -> Assert.False(true)
    | Ok events ->
        Assert.True(events.Length > 0)
