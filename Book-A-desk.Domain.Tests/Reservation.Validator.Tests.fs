module Book_A_Desk.Domain.Reservation.Tests

open System
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let getOffices () = List.Empty
    
let aReservationAggregate =
    {
        Id = ReservationAggregate.Id
        BookedDesks = []
    }

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH an empty email address, WHEN validating, THEN validation should fail`` () =
    let commandWithEmptyEmailAddress =
        {
            EmailAddress = EmailAddress ""
            Date = DateTime.MaxValue
            OfficeId = OfficeId (Guid.NewGuid())
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOffices commandWithEmptyEmailAddress aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because email is empty"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH a date in the past, WHEN validating, THEN validation should fail`` () =
    let commandWithPastDate =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MinValue
            OfficeId = OfficeId (Guid.NewGuid())
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOffices commandWithPastDate aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because date is in the past"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH an invalid office id, WHEN validating, THEN validation should fail`` () =
    let getOfficesHasAnOffice () = []
    
    let commandWithInvalidOfficeId =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = OfficeId Guid.Empty
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOfficesHasAnOffice commandWithInvalidOfficeId aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because the office id is invalid"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH no desks available, WHEN validating, THEN validation should fail`` () =
    let office = { An.office with BookableDesksPerDay = 1 }
    let getOfficesHasAnOffice () = [office]
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let aReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [{ A.booking with OfficeId = office.Id}]
        }
    
    let result = BookADeskReservationValidator.validateCommand getOfficesHasAnOffice command aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because all reservations are taken"
    | Error _ -> ()
    
[<Fact>]
let ``GIVEN A valid Book-A-Desk Reservation command, WHEN validating the command, THEN validation should pass.`` () =
    let office = An.office
    let getOfficesHasAnOffice () = [office]
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOfficesHasAnOffice command aReservationAggregate
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()