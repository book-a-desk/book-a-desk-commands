module Book_A_Desk.Domain.Tests.FilteredBookingsTest

open System
open Xunit

open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Queries
open Book_A_Desk.Domain.Reservation.Events

let officeId = Guid.NewGuid ()
let date = DateTime(2100,02,01)

let createDeskBookedEvent date officeId email : DomainEvent =
    ({
            Date = date
            EmailAddress = email |> EmailAddress
            OfficeId = officeId |> OfficeId
    }: Events.DeskBooked) |> DeskBooked |> ReservationEvent

let bookings : DomainEvent seq = 
    [
        createDeskBookedEvent date officeId "lasnikr"
        createDeskBookedEvent date officeId "user"
        createDeskBookedEvent date officeId "dummy"
    ]

let getBookingsFrom (bookingEvents : seq<DomainEvent>) (date : DateTime option)  (officeId : OfficeId option) (email : EmailAddress option) =
    let result = ReservationsQueriesHandler.getFilteredBookings bookingEvents date officeId email
    match result with
    | Ok bookings -> bookings
    | Error _ -> failwith "Error occured"

[<Fact>]
let ``GIVEN booking events and a date WHEN filtering bookings THEN all bookings for the date are returned`` () = async {
    let bookingEvents = bookings
    let bookings = getBookingsFrom bookingEvents (Some date) None None
    
    Assert.Equal(bookings.Length, 3)
    Assert.Equal(date, bookings[0].Date)
    Assert.Equal(date, bookings[1].Date)
    Assert.Equal(date, bookings[2].Date)
}

// [<Fact>]
// let ``GIVEN booking events and an email WHEN filtering bookings THEN all bookings for the email are returned`` () = async {
// }

// [<Fact>]
// let ``GIVEN booking events and an officeId WHEN filtering bookings THEN all bookings for the officeId are returned`` () = async {
// }

// [<Fact>]
// let ``GIVEN booking events, an officeId and an email WHEN filtering bookings THEN all bookings for the officeId are returned`` () = async {
// }