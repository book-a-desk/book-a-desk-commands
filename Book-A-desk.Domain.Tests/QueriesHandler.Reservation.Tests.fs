module Book_A_desk.Domain.Reservation.Queries.Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events
open Book_A_Desk.Domain.Reservation.Queries
open Xunit

[<Fact>]
let ``GIVEN A ReservationQueriesHandler, WHEN getting the booking, THEN booking is returned`` () =
    let email = "unit@test.com" |> EmailAddress
    let date = DateTime(2100,02,01)
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    let aBooking =
        ({  
            Date = date
            EmailAddress = email 
            OfficeId = officeId 
        } : Reservation.Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let result = ReservationsQueriesHandler.getUserBookingsStartFrom [aBooking] email date
    
    match result with
    | Error _ -> failwith "bookings should have been returned"
    | Ok [booking] ->
        Assert.Equal(email, booking.EmailAddress)
        Assert.Equal(date, booking.Date)
        Assert.Equal(officeId, booking.OfficeId)
    | Ok _ -> failwith "bookings should have been the one given"
    
[<Fact>]
let ``GIVEN A ReservationQueriesHandler, WHEN getting the booking and no booking for user, THEN booking is not returned`` () =
    let email = "unit@test.com" |> EmailAddress
    let notMeEmail = "notme@test.com" |> EmailAddress
    let date = DateTime(2100,02,01)
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    let aBooking =
        ({  
            Date = date
            EmailAddress = email 
            OfficeId = officeId 
        } : Reservation.Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let result = ReservationsQueriesHandler.getUserBookingsStartFrom [aBooking] notMeEmail date
    
    match result with
    | Error _ -> failwith "bookings should have been returned"
    | Ok bookings ->
        Assert.Empty(bookings)