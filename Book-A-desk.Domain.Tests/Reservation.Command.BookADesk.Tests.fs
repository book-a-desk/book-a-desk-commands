module Book_A_Desk.Domain.Reservation.Commands.Tests

open System
open Book_A_Desk.Domain.Reservation.Events
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let getAllOffices = Offices.All
let domainName = "domain.com"

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN executing the command and desks are available, THEN an event should be created`` () =
    let command = BookADeskReservationCommand.provide getAllOffices domainName

    let office = Offices.All.[0]
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress $"email@{domainName}"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeId = office.Id
    }


    let result = command.ExecuteWith bookADesk A.reservationAggregate
    match result with
    | Error _ -> Assert.False(true)
    | Ok events ->
        Assert.True(events.Length > 0)

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN executing the command and desks are available, THEN the correct event is created`` () =
    let command = BookADeskReservationCommand.provide getAllOffices domainName

    let office = Offices.All.[0]
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress $"email@{domainName}"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeId = office.Id
    }

    let result = command.ExecuteWith bookADesk A.reservationAggregate
    match result with
    | Error _ -> Assert.False(true)
    | Ok events ->
        Assert.True(events |> List.forall (function | DeskBooked _ -> true ))
