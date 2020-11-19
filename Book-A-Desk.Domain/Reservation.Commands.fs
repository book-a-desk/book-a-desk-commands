namespace Book_A_Desk.Domain.Reservation.Commands

open System

open Book_A_Desk.Domain

//Commands
type BookADesk =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeID: OfficeID
    }

type ReservationCommand =
    | BookADesk of BookADesk
//    | UnbookAdesk Of UnbookAdesk

