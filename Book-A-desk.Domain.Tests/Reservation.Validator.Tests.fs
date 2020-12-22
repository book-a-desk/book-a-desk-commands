module Book_A_Desk.Domain.Reservation.Tests

open System
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let getOffices () = List.Empty

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation validator, WHEN validating the command, THEN I expect E-Mail address is not empty`` () =
    let commandWithEmptyEmailAddress =
        {
            EmailAddress = EmailAddress ""
            Date = DateTime.MaxValue
            OfficeId = OfficeId (Guid.NewGuid())
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOffices commandWithEmptyEmailAddress None
    match result with
    | Ok _ -> failwith "Validation should fail because email is empty"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect Date must be greater then today`` () =
    let commandWithPastDate =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MinValue
            OfficeId = OfficeId (Guid.NewGuid())
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOffices commandWithPastDate None
    match result with
    | Ok _ -> failwith "Validation should fail because date is in the past"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN I expect valid office id`` () =
    let getOfficesHasAnOffice () = [Office.Create]
    
    let commandWithInvalidOfficeId =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = OfficeId Guid.Empty
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand getOfficesHasAnOffice commandWithInvalidOfficeId None
    match result with
    | Ok _ -> failwith "Validation should fail because the office id is invalid"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command, WHEN validating the command, THEN ensure only specified number of bookings per office is allowed.`` () =
    let office = { Office.Create with BookableDesksPerDay = 1 }
    let getOfficesHasAnOffice () = [office]
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let someReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [{ Booking.Create with OfficeId = office.Id}]
        }
        |> Some
    
    let result = BookADeskReservationValidator.validateCommand getOfficesHasAnOffice command someReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because all reservations are taken"
    | Error _ -> ()
    
[<Fact>]
let ``GIVEN A valid Book-A-Desk Reservation command, WHEN validating the command, THEN should validate.`` () =
    let office = Office.Create
    let getOfficesHasAnOffice () = [office]
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let someReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = []
        }
        |> Some
    
    let result = BookADeskReservationValidator.validateCommand getOfficesHasAnOffice command someReservationAggregate
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()