namespace Book_A_Desk.Domain.Reservation.Queries

open System
open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation

module rec ReservationsQueriesHandler =
    let get (bookingEvents : seq<DomainEvent>) (date : DateTime) : Result<Booking list, string> = result {
        let bookingEvents =
                bookingEvents
                |> Seq.map (function | ReservationEvent event -> event)
        
        let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
        let isSameDate = isSameDate date
        return List.where (fun booking -> isSameDate booking.Date) bookings
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
        
    let private isSameDate (date1:DateTime) (date2:DateTime) =
        date1.Day = date2.Day && date1.Month = date2.Month && date1.Year = date2.Year