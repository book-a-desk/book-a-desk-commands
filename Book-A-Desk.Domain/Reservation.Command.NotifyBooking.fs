module Book_A_Desk.Domain.Notification_Command_NotifyBooking

open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Events

type BookADeskNotificationCommand =
    {
        ExecuteWith: NotifyBooking -> ReservationAggregate -> Result<ReservationEvent list, string>
    }

module BookADeskNotificationCommand =
    let provide sendEmail =
        let validate (command:NotifyBooking) (reservationAggregate:ReservationAggregate)  =
             //BookADeskReservationValidator.validateCommand xxx command reservationAggregate
             Ok ()

        let execute (command:NotifyBooking) (reservationAggregate:ReservationAggregate) =
            let emailMessage =
                {
                    ToAddress = command.EmailAddress
                    Subject =  EmailSubject "Book A Desk Confirmation"
                    Body = EmailBody $"You booked a desk at date: %s{command.Date.ToShortDateString()} in the office in  %s{command.OfficeId.ToString()}"
                }
            let result =
                sendEmail emailMessage

            match result with
            | Error x -> Error "Could not send email!"
            | Ok _ ->
                {
                    BookingNotified.ReservationId = ReservationAggregate.Id
                    Date = command.Date
                    EmailAddress = command.EmailAddress
                    OfficeId = command.OfficeId
                }
                |> BookingNotified
                |> List.singleton

        let executeWith cmd reservationAggregate =
            (validate cmd reservationAggregate)
            |> Result.map (execute cmd)


        {
            ExecuteWith = executeWith
        }
