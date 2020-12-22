module Book_A_Desk.Domain.Reservation.Commands.Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Xunit

let initialReservationAggregate =
    {
        Id = ReservationAggregate.Id
        BookedDesks= []
    }
    |> Some

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect E-Mail address is not empty`` () =
    let command = BookADeskReservationCommand.provide ()
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress ""
        BookADesk.Date = DateTime.Now
        BookADesk.OfficeId = Offices.All.[0].Id
    }
    let result = command.ExecuteWith bookADesk initialReservationAggregate
    match result with
    | Error e -> Assert.Equal("The e-mail address must not be empty.", e)
    | Ok _ -> Assert.False(true)

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect Date must be greater then today`` () =
    let command = BookADeskReservationCommand.provide ()
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(-1.)
        BookADesk.OfficeId = Offices.All.[0].Id
    }
    let result = command.ExecuteWith bookADesk initialReservationAggregate
    match result with
    | Error e -> Assert.Equal("Date must be greater than today.", e)
    | Ok _ -> Assert.False(true)


[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect valid office id`` () =
    let command = BookADeskReservationCommand.provide ()
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeId = OfficeId Guid.Empty
    }
    let result = command.ExecuteWith bookADesk initialReservationAggregate
    match result with
    | Error e -> Assert.Equal("You must enter a valid office ID.", e)
    | Ok _ -> Assert.False(true)

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN ensure only specified number of bookings per office is allowed.`` () =
    let command = BookADeskReservationCommand.provide ()

    let office = Offices.All.[0]
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeId = office.Id
    }

    let aggregateWithTenBookings =
        {
            Id = ReservationAggregate.Id
            BookedDesks =
                List.init office.BookableDesksPerDay (fun index -> { OfficeId = office.Id; Date = bookADesk.Date; EmailAddress = ("email" + index.ToString()) |> EmailAddress  })
        }
        |> Some

    let result = command.ExecuteWith bookADesk aggregateWithTenBookings
    match result with
    | Error e -> Assert.Equal((sprintf "The office is booked out at %s" (bookADesk.Date.ToShortDateString())), e)
    | Ok _ -> Assert.False(true)

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command and desks are available, THEN an event should be created`` () =
    let command = BookADeskReservationCommand.provide ()

    let office = Offices.All.[0]
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeId = office.Id
    }

    let result = command.ExecuteWith bookADesk initialReservationAggregate
    match result with
    | Error e -> Assert.False(true)
    | Ok events ->
        Assert.True(events.Length > 0)
