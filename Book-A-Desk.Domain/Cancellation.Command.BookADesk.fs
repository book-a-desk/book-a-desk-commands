namespace Book_A_Desk.Domain.Cancellation.Commands

open Book_A_Desk.Domain.Cancellation
open Book_A_Desk.Domain.Errors
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Events

type BookADeskCancellationCommand =
    {
        ExecuteWith: CancelBookADesk -> ReservationAggregate -> Result<ReservationEvent list, ReservationError>
    }

module BookADeskCancellationCommand =
    let provide offices domainName =
        let validate (command:CancelBookADesk) reservationAggregate  =
             BookADeskCancellationValidator.validateCommand offices command reservationAggregate domainName

        let execute (command:CancelBookADesk) reservationAggregate =
            {
                Date = command.Date
                EmailAddress = command.EmailAddress
                OfficeId = command.OfficeId
            }
            |> DeskCancelled
            |> List.singleton

        let executeWith cmd reservationAggregate =
            (validate cmd reservationAggregate)
            |> Result.map (execute cmd)

        {
            ExecuteWith = executeWith
        }
