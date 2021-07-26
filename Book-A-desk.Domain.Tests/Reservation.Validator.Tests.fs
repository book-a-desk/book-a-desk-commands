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
let domainName = "domain.com"
    
let aReservationAggregate =
    {
        Id = ReservationAggregate.Id
        BookedDesks = []
    }
    
type InvalidEmails() as this =
    inherit TheoryData<string>()
    do  this.Add("")
    do  this.Add(String.Empty)
    do  this.Add("john.smith@forbidden.com")
    do  this.Add("JOHN.SMITH@FORBIDDEN.COM")
    do  this.Add("jsmith@forbidden.com")
    do  this.Add("JSMITH@FORBIDDEN.COM")
    do  this.Add("jsmith")
    do  this.Add("jsmith@")
    do  this.Add("@forbidden.com")

[<Theory; ClassData(typeof<InvalidEmails>)>]
let ``GIVEN A Book-A-Desk Reservation command WITH an invalid corporate email address, WHEN validating, THEN validation should fail`` (email:string) =
    let commandWithEmptyEmailAddress =
        {
            EmailAddress = EmailAddress email
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let validDomainName = "domain.com"
    let result = BookADeskReservationValidator.validateCommand offices commandWithEmptyEmailAddress aReservationAggregate validDomainName
    match result with
    | Ok _ -> failwith "Validation should fail because email is not valid"
    | Error _ -> ()

type ValidEmails() as this =
    inherit TheoryData<string>()
    do  this.Add("john.smith@allowed.com")
    do  this.Add("JOHN.SMITH@ALLOWED.COM")
    do  this.Add("jsmith@allowed.com")
    do  this.Add("JSMITH@ALLOWED.COM")
[<Theory; ClassData(typeof<ValidEmails>)>]
let ``GIVEN A Book-A-Desk Reservation command WITH a valid corporate email address, WHEN validating, THEN validation should pass`` (email:string) =
    let commandWithEmptyEmailAddress =
        {
            EmailAddress = EmailAddress email
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let validDomainName = "allowed.com"
    let result = BookADeskReservationValidator.validateCommand offices commandWithEmptyEmailAddress aReservationAggregate validDomainName
    match result with
    | Ok _ -> ()
    | Error _ -> failwith "Validation should pass because email is valid"
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
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = bookedDate
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithPastDate aReservationAggregate domainName
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
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = requestedDate
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithPastDate aReservationAggregate domainName
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH an invalid office id, WHEN validating, THEN validation should fail`` () =
    let commandWithInvalidOfficeId =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = OfficeId Guid.Empty
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices commandWithInvalidOfficeId aReservationAggregate domainName
    match result with
    | Ok _ -> failwith "Validation should fail because the office id is invalid"
    | Error _ -> ()

[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command WITH no desks available, WHEN validating, THEN validation should fail`` () =
    let office = { An.office with BookableDesksPerDay = 1 }
    let offices = [office]
    let command =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let aReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [{ A.booking with OfficeId = office.Id}]
        }
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate domainName
    match result with
    | Ok _ -> failwith "Validation should fail because all reservations are taken"
    | Error _ -> ()
    
[<Fact>]
let ``GIVEN A valid Book-A-Desk Reservation command, WHEN validating the command, THEN validation should pass.`` () =
    let command =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate domainName
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()
    
[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command with the user not already booked, WHEN validating the command, THEN validation should pass.`` () =
    let command =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : BookADesk
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate domainName
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()
    
[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command with the user booked on a different date, WHEN validating the command, THEN validation should pass.`` () =
    let emailAddress = $"email@{domainName}"
    let bookedDate = DateTime.MaxValue.AddDays(-1.0)
    let newDate = DateTime.MaxValue
    let command =
        {
            EmailAddress = EmailAddress emailAddress
            Date = bookedDate
            OfficeId = office.Id
        } : BookADesk
    
    let aReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = emailAddress |> EmailAddress
                                    Date = newDate
                                }
                            ]
        }
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate domainName
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()
    
[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command with the user booked in a different office, WHEN validating the command, THEN validation should pass.`` () =
    let emailAddress = $"email@{domainName}"
    let bookedDate = DateTime.MaxValue
    let newOffice = An.newOffice
    let command =
        {
            EmailAddress = EmailAddress emailAddress
            Date = bookedDate
            OfficeId = office.Id
        } : BookADesk
    
    let aReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = newOffice.Id
                                    EmailAddress = emailAddress |> EmailAddress
                                    Date = bookedDate
                                }
                            ]
        }
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate domainName
    match result with
    | Error _ -> failwith "Validation should have succeeded"
    | Ok _ -> ()
    
[<Fact>]
let ``GIVEN A Book-A-Desk Reservation command with the user already booked, WHEN validating the command, THEN validation should fail.`` () =
    let emailAddress = $"email@{domainName}"
    let bookedDate = DateTime.MaxValue
    let command =
        {
            EmailAddress = EmailAddress emailAddress
            Date = bookedDate
            OfficeId = office.Id
        } : BookADesk
    
    let aReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = emailAddress |> EmailAddress
                                    Date = bookedDate
                                }
                            ]
        }
    
    let result = BookADeskReservationValidator.validateCommand offices command aReservationAggregate domainName
    match result with
    | Ok _ -> failwith "Validation should fail because user already booked on that day"
    | Error _ -> ()