module Book_A_Desk.Domain.Notification_Commands

open Book_A_Desk.Domain.Reservation.Events

//Commands
type NotifyBooking =
    {
        DeskBooked: DeskBooked
    }

type NotificationCommand =
    | NotifyBooking of NotifyBooking

