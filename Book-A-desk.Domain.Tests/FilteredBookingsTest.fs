module Book_A_Desk.Domain.Tests.FilteredBookingsTest

open System
open Xunit

open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Queries
open Book_A_Desk.Domain.Reservation.Events

let email = "lasnikr"
let altEmail = "dummy"
let office = Guid.Parse("11111111-1111-1111-1111-111111111111")
let altOffice = Guid.Parse("22222222-2222-2222-2222-222222222222")
let date = DateTime(2000,05,03)
let altDate = DateTime(2100,05,03)

let createDeskBookedEvent date officeId email : DomainEvent =
    ({
            Date = date
            EmailAddress = email |> EmailAddress
            OfficeId = officeId |> OfficeId
    }: Events.DeskBooked) |> DeskBooked |> ReservationEvent

let bookingEvents : DomainEvent seq = 
    [
        createDeskBookedEvent altDate   office    email
        createDeskBookedEvent date      altOffice email
        createDeskBookedEvent date      office    altEmail
    ]

let getBookingsFrom (bookingEvents : seq<DomainEvent>) (date : DateTime option)  (officeId : OfficeId option) (email : EmailAddress option) =
    let result = ReservationsQueriesHandler.getFilteredBookings bookingEvents date officeId email
    match result with
    | Ok bookings -> bookings
    | Error _ -> failwith "Error occured"

[<Fact>]
let ``GIVEN booking events and a date WHEN filtering bookings THEN all bookings for the date are returned`` () = async {
    let bookings = getBookingsFrom bookingEvents (Some date) None None
    
    Assert.Equal(bookings.Length, 2)
    Assert.Equal(date, bookings[0].Date)
    Assert.Equal(date, bookings[1].Date)
}

[<Fact>]
let ``GIVEN booking events and an officeId WHEN filtering bookings THEN all bookings for the officeId are returned`` () = async {
    let officeId = office |> OfficeId

    let bookings = getBookingsFrom bookingEvents None (Some officeId) None
    
    Assert.Equal(bookings.Length, 2)
    Assert.Equal(officeId, bookings[0].OfficeId)
    Assert.Equal(officeId, bookings[1].OfficeId)
}

[<Fact>]
let ``GIVEN booking events and an email WHEN filtering bookings THEN all bookings for the email are returned`` () = async {
    let emailAddress = email |> EmailAddress

    let bookings = getBookingsFrom bookingEvents None None (Some emailAddress)
    
    Assert.Equal(bookings.Length, 2)
    Assert.Equal(emailAddress, bookings[0].EmailAddress)
    Assert.Equal(emailAddress, bookings[1].EmailAddress)
}

[<Fact>]
let ``GIVEN booking events, an officeId and an email WHEN filtering bookings THEN all bookings for the officeId and email are returned`` () = async {
    let officeId = office |> OfficeId
    let emailAddress = email |> EmailAddress

    let bookings = getBookingsFrom bookingEvents None (Some officeId) (Some emailAddress)

    Assert.Equal(bookings.Length, 1)
    Assert.Equal(emailAddress, bookings[0].EmailAddress)
    Assert.Equal(officeId, bookings[0].OfficeId)
}