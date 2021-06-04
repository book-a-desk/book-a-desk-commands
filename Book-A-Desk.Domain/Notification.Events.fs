module Book_A_Desk.Domain.Notification_Events

open Book_A_Desk.Domain.Reservation.Events

type BookNotified =
    {
        DeskBooked: DeskBooked
    }

type NotificationEvent =
    | BookNotified of BookNotified
