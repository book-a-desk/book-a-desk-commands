namespace Book_A_Desk.Domain.Reservation.Commands

open System

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain

//Commands
type BookADesk =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeId: OfficeId
    }

type ReservationCommand =
    | BookADesk of BookADesk
    //| CancelADesk of CancelADesk

