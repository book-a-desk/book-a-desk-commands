namespace Book_A_Desk.Infrastructure

open System
open Book_A_Desk.Domain.Reservation.Events
open FSharp.AWS.DynamoDB

type ReservationEvent =
    {
        [<HashKey>]
        AggregateId : Guid
        ReservationType : ReservationType
        Event: string
    }
