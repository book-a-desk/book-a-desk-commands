namespace Book_A_Desk.Domain.Reservation

open System
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events

type ReservationAggregate =
    {
        Id: ReservationId
        ReservationEvents: ReservationEvent list
    }

// ReservationAggregate will be defined at Booking Level
module ReservationAggregate =

    let applyEventTo reservationAggregate (event: ReservationEvent) =
        { reservationAggregate with
            ReservationEvents = event :: reservationAggregate.ReservationEvents
        }

    let getAggregateFrom reservationCommand = NotImplementedException

    let getCurrentStateFrom (event: ReservationEvent) =
        // TODO: From event get ReservationAggregate and last ReservationEvent
        // TODO: Evaluate if getCurrentStateFrom can filter by OfficeId, EmailAddress and Date
        event
                   
