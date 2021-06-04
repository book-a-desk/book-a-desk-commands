namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Notification_Commands

type NotificationsCommandHandler =
    {
        Handle: NotificationCommand -> Result<unit,string>
    }

module NotificationCommandHandler =
   let provide (eventStore:EventStore) notificationCommandsFactory =

       let handle (command : NotificationCommand) =
            let storeEventsForBatch events =
                events
                |> List.singleton
                |> Map.ofList
                |> eventStore.AppendEvents

            let run executeCommandWith cmd = result {
                let! events = eventStore.GetEvents
                let! commandResult =
                    events
                    |> List.map (function
                        | NotificationEvent event -> event
                        | otherType -> failwithf $"There is an unexpected event type %A{otherType}"
                     )
                    |> executeCommandWith cmd
                return storeEventsForBatch commandResult
            }

            match command with
            | NotifyBooking command ->
                let commandExecutor = notificationCommandsFactory.CreateBookADeskNotificationCommand ()
                run commandExecutor.ExecuteWith command

       {
            Handle = handle
       }
