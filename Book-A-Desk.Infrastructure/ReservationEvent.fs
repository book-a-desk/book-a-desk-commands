namespace Book_A_Desk.Infrastructure

open System
open FSharp.AWS.DynamoDB

type ReservationType =
    | DeskBooked
    | DeskCancelled

type ReservationEvent =
    {
        [<HashKey>]
        AggregateId : Guid
        ReservationType : ReservationType
        Event: string
    }
    
type DeskBooked =
    {
        Date: DateTime
        EmailAddress: string
        OfficeId: Guid
    }
    
type DeskCancelled =
    {
        Date: DateTime
        EmailAddress: string
        OfficeId: Guid
    }

