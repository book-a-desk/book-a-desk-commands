namespace Book_A_Desk.Domain.Reservation.Commands

open System

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Domain

//Commands
type BookADesk =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeId: OfficeId
    }
type CancelBookADesk =
    {
        AggregateId: ReservationId
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeId: OfficeId
    }

type ReservationCommand =
    | BookADesk of BookADesk
    | CancelBookADesk of CancelBookADesk