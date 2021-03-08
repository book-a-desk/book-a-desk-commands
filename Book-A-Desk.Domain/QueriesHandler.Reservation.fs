namespace Book_A_Desk.Domain.Reservation.Queries

open System
open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain

module rec ReservationsQueriesHandler =
    let get (eventStore : EventStore) (date : DateTime) : Result<Booking seq, string> Async = async {
        let (ReservationId aggregateId) = ReservationAggregate.Id
        let! bookingEvents = eventStore.GetEvents aggregateId
        match bookingEvents with
        | Ok bookingEvents -> 
            let bookingEvents = bookingEvents
                                |> Seq.map (function | ReservationEvent event -> event)
            
            let bookings = (ReservationAggregate.getCurrentStateFrom bookingEvents).BookedDesks
            let isSameDate = isSameDate date
            return Ok (Seq.where (fun booking -> isSameDate booking.Date) bookings)
        | Error e -> return (Error e)
    }
        
    let private isSameDate date1 date2 =
        date1.Day = date2.Day && date1.Month = date2.Month && date1.Year = date2.Year