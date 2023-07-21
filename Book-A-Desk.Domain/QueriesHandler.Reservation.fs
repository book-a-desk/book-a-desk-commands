namespace Book_A_Desk.Domain.Reservation.Queries

open System
open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Office.Domain

module rec ReservationsQueriesHandler =
    let get (bookingEvents : seq<DomainEvent>) (date : DateTime) : Result<Booking list, string> = result {
        let bookingEvents =
                bookingEvents
                |> Seq.map (function | ReservationEvent event -> event)
        
        let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
        let isSameDate = isSameDate date
        return List.where (fun booking -> isSameDate booking.Date) bookings
    }

    let getUserBookingsByOfficeFrom (bookingEvents : seq<DomainEvent>) (date : DateTime) (office : OfficeId) : Result<Booking list, string> = result {
        let bookingEvents =
                bookingEvents
                |> Seq.map (function | ReservationEvent event -> event)
        
        let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
        let isSameDate = isSameDate date
        return List.where (fun booking -> isSameDate booking.Date && booking.OfficeId = office) bookings
    }
    
    let getUserBookingsStartFrom (bookingEvents : seq<DomainEvent>) (email : EmailAddress) (date : DateTime) : Result<Booking list, string> = result {
        let bookingEvents =
                bookingEvents
                |> Seq.map (function | ReservationEvent event -> event)
        
        let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
        return List.where (fun booking -> booking.EmailAddress = email && booking.Date >= date) bookings
    }
    
    let getUsersBookingsStartFrom (bookingEvents : seq<DomainEvent>) (date : DateTime) : Result<Booking list, string> = result {
        let bookingEvents =
                bookingEvents
                |> Seq.map (function | ReservationEvent event -> event)
        
        let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
        return List.where (fun booking -> booking.Date >= date) bookings
    }

    let getFilteredBookings (bookingEvents : seq<DomainEvent>) (date : DateTime option)  (officeId : OfficeId option) (email : EmailAddress option) : Result<Booking list, string> = result {
        let bookingEvents =
                bookingEvents
                |> Seq.map (function | ReservationEvent event -> event)
        
        let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
        return 
            bookings
            |> List.where (fun booking -> if date.IsNone then true else date.Value = booking.Date)
            |> List.where (fun booking -> if officeId.IsNone then true else officeId.Value = booking.OfficeId)
            |> List.where (fun booking -> if email.IsNone then true else email.Value = booking.EmailAddress)
    }
        
    let private isSameDate date1 date2 =
        date1.Day = date2.Day && date1.Month = date2.Month && date1.Year = date2.Year