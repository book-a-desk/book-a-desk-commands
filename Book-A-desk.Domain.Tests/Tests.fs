module Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Reservation.Commands
open Xunit

let validationResultsOf f = f()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect E-Mail address is not empty`` () =
    let command = BookADeskReservationCommand.provide validationResultsOf
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress ""
        BookADesk.Date = DateTime.Now
        BookADesk.OfficeID = OfficeID ""
    }
    let result = command.ExecuteWith bookADesk
    match result with
    | Error e -> Assert.Equal("The e-mail address must not be empty.", e)
    | Ok _ -> Assert.False(true)
    
[<Fact>]    
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect Date must be greater then today`` () =
    let command = BookADeskReservationCommand.provide validationResultsOf
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(-1.)
        BookADesk.OfficeID = OfficeID ""
    }
    let result = command.ExecuteWith bookADesk
    match result with
    | Error e -> Assert.Equal("Date must be greater than today.", e)
    | Ok _ -> Assert.False(true)
    
    
[<Fact>]    
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect valid office id`` () =
    let command = BookADeskReservationCommand.provide validationResultsOf
    let bookADesk = {
        BookADesk.EmailAddress = EmailAddress "something@something.com"
        BookADesk.Date = DateTime.Now.AddDays(1.)
        BookADesk.OfficeID = OfficeID "Toronto"
    }
    let result = command.ExecuteWith bookADesk
    match result with
    | Error e -> Assert.Equal("You must enter a valid office ID.", e)
    | Ok _ -> Assert.False(true)    
    
    
    