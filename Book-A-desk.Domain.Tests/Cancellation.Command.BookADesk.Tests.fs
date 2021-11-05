module Book_A_Desk.Domain.Cancellation.Commands.Tests

open System
open Book_A_Desk.Domain.Reservation.Events
open Xunit

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_desk.Domain.Tests

let getAllOffices = Offices.All
let domainName = "domain.com"

[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command, WHEN executing the command and desks are available, THEN an event should be created`` () =
    let emailAddress = $"email@{domainName}"
    let bookedDate = DateTime.Now.AddDays(1.)
    let office = Offices.All.[0]
    let bookedReservationAggregate =
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
    let command = BookADeskCancellationCommand.provide getAllOffices domainName

    
    let cancelBookADesk = {
        CancelBookADesk.EmailAddress = EmailAddress $"email@{domainName}"
        CancelBookADesk.Date = bookedDate
        CancelBookADesk.OfficeId = office.Id
    }


    let result = command.ExecuteWith cancelBookADesk bookedReservationAggregate
    match result with
    | Error _ -> Assert.False(true)
    | Ok events ->
        Assert.True(events.Length > 0)

[<Fact>]
let ``GIVEN A Book-A-Desk Cancellation command, WHEN executing the command and desks are available, THEN the correct event is created`` () =
    let emailAddress = $"email@{domainName}"
    let bookedDate = DateTime.Now.AddDays(1.)
    let office = Offices.All.[0]
    let bookedReservationAggregate =
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
    let command = BookADeskCancellationCommand.provide getAllOffices domainName

    
    let cancelBookADesk = {
        CancelBookADesk.EmailAddress = EmailAddress $"email@{domainName}"
        CancelBookADesk.Date = DateTime.Now.AddDays(1.)
        CancelBookADesk.OfficeId = office.Id
    }

    let result = command.ExecuteWith cancelBookADesk bookedReservationAggregate
    match result with
    | Error _ -> Assert.False(true)
    | Ok events ->
        Assert.True(events |> List.forall (function | DeskBooked _ -> false | DeskCancelled _ -> true))
