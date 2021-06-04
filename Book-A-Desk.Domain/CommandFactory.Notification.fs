namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Notification_Command_NotifyBooking

type NotificationCommandsFactory =
    {
        CreateBookADeskNotificationCommand: unit -> BookADeskNotificationCommand
    }

module NotificationCommandsFactory =
    let provide getOffices =

        let createBookADeskNotificationCommand () = BookADeskNotificationCommand.provide

        {
            CreateBookADeskNotificationCommand = createBookADeskNotificationCommand
        }
