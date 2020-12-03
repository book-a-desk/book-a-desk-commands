namespace Book_A_Desk.Domain.Reservation.Events

open System

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Office.Domain

type DeskBooked =
    {
        ReservationId: ReservationId
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeId: OfficeId
    }

type ReservationEvent =
    | DeskBooked of DeskBooked
