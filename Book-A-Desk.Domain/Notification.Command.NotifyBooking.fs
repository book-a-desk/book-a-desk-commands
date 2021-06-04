module Book_A_Desk.Domain.Notification_Command_NotifyBooking

open Book_A_Desk.Domain.Notification_Events
open Book_A_Desk.Domain.Reservation.Events

type BookADeskNotificationCommand =
    {
        ExecuteWith: DeskBooked -> NotificationEvent
    }

module BookADeskNotificationCommand =
    let provide =
        // Send an email and provide NotifiedBooking
        let execute (deskBooked:DeskBooked) =
            {
                DeskBooked = deskBooked
            }
            |> BookNotified

        let executeWith deskBooked =
            execute deskBooked

        {
            ExecuteWith = executeWith 
        }
    