namespace Book_A_Desk.Domain.Reservation.Events

open System

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Reservation.Domain

type DeskBooked =
    {
        ReservationId: ReservationId
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeID: OfficeID
    }

type ReservationEvent =
    | DeskBooked of DeskBooked
