namespace Book_A_Desk.Infrastructure

open System
open FSharp.AWS.DynamoDB

type ReservationType =
    | DeskBooked

type ReservationEvent =
    {
        AggregateId : Guid
        ReservationType : ReservationType
    }
    
type DeskBooked =
    {
        [<HashKey>]
        AggregateId : Guid
        [<RangeKey>]
        EventId : Guid
        Date: DateTimeOffset
        EmailAddress: string
        OfficeId: Guid
    }

