module Book_A_Desk.Domain.Reservation.Tests

open System
open Book_A_Desk.Core
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let office = An.office
let offices = [office]
    
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
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithEmptyEmailAddress aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because email is empty"
    | Error _ -> ()

type ForbiddenDatesForPastAndToday() as this =
    inherit TheoryData<DateTime>()
    do  this.Add(DateTime.MinValue)
    do  this.Add(DateTime.Today.AddSeconds(-1.))
    do  this.Add(DateTime.Now.AddMinutes(-1.))
    do  this.Add(DateTime.Today.AddHours(00.).AddMinutes(00.).AddSeconds(00.))
    do  this.Add(DateTime.Today.AddHours(00.).AddMinutes(00.).AddSeconds(01.))
    do  this.Add(DateTime.Today.AddHours(23.).AddMinutes(59.).AddSeconds(59.))

[<Theory; ClassData(typeof<ForbiddenDatesForPastAndToday>)>]
let ``GIVEN A Book-A-Desk Reservation command WITH a past or today date, WHEN validating, THEN validation should fail`` (bookedDate:DateTime) =
    let commandWithPastDate =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = bookedDate
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithPastDate aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because date is in the past"
    | Error _ -> ()
    
type AllowedFutureDates() as this =
    inherit TheoryData<DateTime>()
    do  this.Add(DateTime.Today.AddDays(1.))
    do  this.Add(DateTime.MaxValue)

[<Theory; ClassData(typeof<AllowedFutureDates>)>]
let ``GIVEN A Book-A-Desk Reservation command WITH a date greater than today, WHEN validating, THEN validation should pass`` (requestedDate:DateTime) =
    let commandWithPastDate =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = requestedDate
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithPastDate aReservationAggregate
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH an invalid office id, WHEN validating, THEN validation should fail`` () =
    let commandWithInvalidOfficeId =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = OfficeId Guid.Empty
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithInvalidOfficeId aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because the office id is invalid"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH no desks available, WHEN validating, THEN validation should fail`` () =
    let office = { An.office with BookableDesksPerDay = 1 }
    let offices = [office]
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
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because all reservations are taken"
    | Error _ -> ()
    
[<Fact>]
let ``GIVEN A valid Book-A-Desk Reservation command, WHEN validating the command, THEN validation should pass.`` () =
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()
    
[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command with the user not already booked, WHEN validating the command, THEN validation should pass.`` () =
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()
    
[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command with the user already booked, WHEN validating the command, THEN validation should fail.`` () =
    let command =
        {
            EmailAddress = EmailAddress "anEmailAddress@fake.com"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let aReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = "anEmailAddress@fake.com" |> EmailAddress
                                }
                            ]
        }
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate
    match result with
    | Ok _ -> failwith "Validation should fail because user already booked on that day"
    | Error _ -> ()