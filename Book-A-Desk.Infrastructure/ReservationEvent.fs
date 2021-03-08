namespace Book_A_Desk.Infrastructure

open System

type ReservationType =
    | DeskBooked

type ReservationEvent =
    {
        AggregateId : Guid
        ReservationType : ReservationType
    }
    
type DeskBooked =
    {
        AggregateId : Guid
        Date: DateTime
        EmailAddress: string
        OfficeId: Guid
    }

