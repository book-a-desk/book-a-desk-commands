module Book_A_Desk.Domain.Cancellation.Tests

open System
open Book_A_Desk.Core
open Book_A_Desk.Domain.Errors
open Book_A_desk.Domain.Helpers.Tests
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let office = An.office
let offices = [office]
let domainName = "domain.com"
    
let emptyReservationAggregate =
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
let ``GIVEN A Book-A-Desk Cancellation command WITH an invalid corporate email address, WHEN validating, THEN validation should fail`` (email:string) =
    let commandWithEmptyEmailAddress =
        {
            EmailAddress = EmailAddress email
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : CancelBookADesk
    
    let validDomainName = "domain.com"
    let result = BookADeskCancellationValidator.validateCommand offices commandWithEmptyEmailAddress emptyReservationAggregate validDomainName    
    result |> Helpers.shouldBeErrorAndEqualTo (ReservationError.InvalidEmailAddress |> Error)

type ValidEmails() as this =
    inherit TheoryData<string>()
    do  this.Add("john.smith@allowed.com")
    do  this.Add("JOHN.SMITH@ALLOWED.COM")
    do  this.Add("jsmith@allowed.com")
    do  this.Add("JSMITH@ALLOWED.COM")
[<Theory; ClassData(typeof<ValidEmails>)>]
let ``GIVEN A Book-A-Desk Cancellation command WITH a valid corporate email address, WHEN validating, THEN validation should pass`` (email:string) =
    
    let bookedReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = email |> EmailAddress
                                }
                            ]
        }
    let commandWithEmptyEmailAddress =
        {
            EmailAddress = EmailAddress email
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : CancelBookADesk
    
    let validDomainName = "allowed.com"
    let result = BookADeskCancellationValidator.validateCommand offices commandWithEmptyEmailAddress bookedReservationAggregate validDomainName    
    result |> Helpers.shouldBeOkAndEqualTo (() |> Ok)
    
type ForbiddenDatesForPastAndToday() as this =
    inherit TheoryData<DateTime>()
    do  this.Add(DateTime.MinValue)
    do  this.Add(DateTime.Today.AddSeconds(-1.))
    do  this.Add(DateTime.Now.AddMinutes(-1.))
    do  this.Add(DateTime.Today.AddHours(00.).AddMinutes(00.).AddSeconds(00.))
    do  this.Add(DateTime.Today.AddHours(00.).AddMinutes(00.).AddSeconds(01.))
    do  this.Add(DateTime.Today.AddHours(23.).AddMinutes(59.).AddSeconds(59.))

[<Theory; ClassData(typeof<ForbiddenDatesForPastAndToday>)>]
let ``GIVEN A Book-A-Desk Cancellation command WITH a past or today date, WHEN validating, THEN validation should fail`` (cancelledDate:DateTime) =
    let commandWithPastDate =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = cancelledDate
            OfficeId = office.Id
        } : CancelBookADesk
    
    let result = BookADeskCancellationValidator.validateCommand offices commandWithPastDate emptyReservationAggregate domainName    
    result |> Helpers.shouldBeErrorAndEqualTo (ReservationError.DateLowerThanToday |> Error)
    
type AllowedFutureDates() as this =
    inherit TheoryData<DateTime>()
    do  this.Add(DateTime.Today.AddDays(1.))
    do  this.Add(DateTime.MaxValue)

[<Theory; ClassData(typeof<AllowedFutureDates>)>]
let ``GIVEN A Book-A-Desk Cancellation command WITH a date greater than today, WHEN validating, THEN validation should pass`` (requestedDate:DateTime) =
    let bookedReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = $"email@{domainName}" |> EmailAddress
                                    Date = requestedDate
                                }
                            ]
        }
    let commandWithPastDate =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = requestedDate
            OfficeId = office.Id
        } : CancelBookADesk
    
    let result = BookADeskCancellationValidator.validateCommand offices commandWithPastDate bookedReservationAggregate domainName    
    result |> Helpers.shouldBeOkAndEqualTo (() |> Ok)

[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command WITH an invalid office id, WHEN validating, THEN validation should fail`` () =
    let commandWithInvalidOfficeId =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = OfficeId Guid.Empty
        } : CancelBookADesk
    
    let result = BookADeskCancellationValidator.validateCommand offices commandWithInvalidOfficeId emptyReservationAggregate domainName
    result |> Helpers.shouldBeErrorAndEqualTo (ReservationError.InvalidOfficeId |> Error)
    
[<Fact>]
let ``GIVEN A valid Book-A-Desk Cancellation command, WHEN validating the command, THEN validation should pass.`` () =
    let bookedReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = $"email@{domainName}" |> EmailAddress
                                }
                            ]
        }
    let command =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : CancelBookADesk
    
    let result = BookADeskCancellationValidator.validateCommand offices command bookedReservationAggregate domainName
    result |> Helpers.shouldBeOkAndEqualTo (() |> Ok)
    
[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command with the user already booked, WHEN validating the command, THEN validation should pass.`` () =
    let office = { An.office with BookableDesksPerDay = 1 }
    let offices = [office]
    
    let bookedReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = office.Id
                                    EmailAddress = $"email@{domainName}" |> EmailAddress
                                }
                            ]
        }
    let command =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : CancelBookADesk
    
    let result = BookADeskCancellationValidator.validateCommand offices command bookedReservationAggregate domainName
    result |> Helpers.shouldBeOkAndEqualTo (() |> Ok)
    
[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command with the user not already booked, WHEN validating the command, THEN validation should fail.`` () =
    let command =
        {
            EmailAddress = EmailAddress $"email@{domainName}"
            Date = DateTime.MaxValue
            OfficeId = office.Id
        } : CancelBookADesk
    
    let userBookingParam : UserBookingParam =
        {
            Date = DateTime.MaxValue
            EmailAddress = EmailAddress $"email@{domainName}"
        }
    
    let result = BookADeskCancellationValidator.validateCommand offices command emptyReservationAggregate domainName
    result |> Helpers.shouldBeErrorAndEqualTo (ReservationError.UserHasNotBookedBefore userBookingParam |> Error)
    
[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command with the user booked on a different date, WHEN validating the command, THEN validation should fail.`` () =
    let emailAddress = $"email@{domainName}"
    let cancelledDate = DateTime.MaxValue.AddDays(-1.0)
    let newDate = DateTime.MaxValue
    let command =
        {
            EmailAddress = EmailAddress emailAddress
            Date = cancelledDate
            OfficeId = office.Id
        } : CancelBookADesk
    
    let bookedReservationAggregate =
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
    
    let userBookingParam : UserBookingParam =
        {
            Date = cancelledDate
            EmailAddress = EmailAddress $"email@{domainName}"
        }
    
    let result = BookADeskCancellationValidator.validateCommand offices command bookedReservationAggregate domainName
    result |> Helpers.shouldBeErrorAndEqualTo (ReservationError.UserHasNotBookedBefore userBookingParam |> Error)
    
[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command with the user booked in a different office, WHEN validating the command, THEN validation should fail.`` () =
    let emailAddress = $"email@{domainName}"
    let cancelledDate = DateTime.MaxValue
    let newOffice = An.anotherOffice
    let command =
        {
            EmailAddress = EmailAddress emailAddress
            Date = cancelledDate
            OfficeId = office.Id
        } : CancelBookADesk
    
    let bookedReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = newOffice.Id
                                    EmailAddress = emailAddress |> EmailAddress
                                    Date = cancelledDate
                                }
                            ]
        }
    
    let userBookingParam : UserBookingParam =
        {
            Date = DateTime.MaxValue
            EmailAddress = EmailAddress $"email@{domainName}"
        }
    
    let result = BookADeskCancellationValidator.validateCommand offices command bookedReservationAggregate domainName
    result |> Helpers.shouldBeErrorAndEqualTo (ReservationError.UserHasNotBookedBefore userBookingParam |> Error)