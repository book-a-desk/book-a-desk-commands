module Book_A_desk.Domain.Reservation_Aggregate.Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events
open Book_A_desk.Domain.Tests
open Xunit

let reservationId = ReservationAggregate.Id
let emailAddress = "email@domain.com" |> EmailAddress
let officeId = Guid.NewGuid() |> OfficeId

[<Fact>]
let ``Given empty bookings When a DeskBooked event is processed Then a Booking is added`` () =
    let aReservationAggregate = A.reservationAggregate
    let bookedDate = DateTime.Today
    
    let deskBookedEvent = ({
        ReservationId = reservationId
        Date = bookedDate
        EmailAddress = emailAddress
        OfficeId = officeId
    } : Events.DeskBooked)
    
    let aReservationAggregate = ReservationAggregate.applyEventTo aReservationAggregate (deskBookedEvent |> ReservationEvent.DeskBooked)
    
    Assert.Equal(1, aReservationAggregate.BookedDesks.Length)
    let bookedDesk = aReservationAggregate.BookedDesks.Head
    Assert.Equal(bookedDesk.OfficeId, deskBookedEvent.OfficeId)
    Assert.Equal(bookedDesk.EmailAddress, deskBookedEvent.EmailAddress)
    Assert.Equal(bookedDesk.Date, deskBookedEvent.Date)

[<Fact>]
// TODO implement test
let ``Given a booking When a DeskBooked event is processed Then there are two bookings stored`` () =
    let aReservationAggregate = A.reservationAggregate
    let bookedDate = DateTime.Today

    let deskBookedEvent = ({
        ReservationId = reservationId
        Date = bookedDate
        EmailAddress = emailAddress
        OfficeId = officeId
    } : Events.DeskBooked)

    let aReservationAggregate = ReservationAggregate.applyEventTo aReservationAggregate (deskBookedEvent |> ReservationEvent.DeskBooked)

    Assert.Equal(1, aReservationAggregate.BookedDesks.Length)
    let bookedDesk = aReservationAggregate.BookedDesks.Head
    Assert.Equal(bookedDesk.OfficeId, deskBookedEvent.OfficeId)
    Assert.Equal(bookedDesk.EmailAddress, deskBookedEvent.EmailAddress)
    Assert.Equal(bookedDesk.Date, deskBookedEvent.Date)

[<Fact>]
let ``Given a booking When a DeskCancelled event is processed Then a Booking is removed`` () =
    let bookedDate = DateTime.Today
    let bookedReservationAggregate =
        {
            Id = ReservationAggregate.Id
            BookedDesks = [
                                { A.booking with
                                    OfficeId = officeId
                                    EmailAddress = emailAddress
                                    Date = bookedDate
                                }
                            ]
        }
    
    let deskBookedEvent = ({
        ReservationId = reservationId
        Date = bookedDate
        EmailAddress = emailAddress
        OfficeId = officeId
    } : Events.DeskCancelled)
    
    let aReservationAggregate = ReservationAggregate.applyEventTo bookedReservationAggregate (deskBookedEvent |> ReservationEvent.DeskCancelled)
    
    Assert.Equal(0, aReservationAggregate.BookedDesks.Length)
    