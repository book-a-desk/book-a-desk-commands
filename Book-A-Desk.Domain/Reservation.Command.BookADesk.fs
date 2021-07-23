namespace Book_A_Desk.Domain.Reservation.Commands

open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events

type BookADeskReservationCommand =
    {
        ExecuteWith: BookADesk -> ReservationAggregate -> Result<ReservationEvent list, string>
    }

module BookADeskReservationCommand =
    let provide offices domainName =
        let validate (command:BookADesk) reservationAggregate  =
             BookADeskReservationValidator.validateCommand offices command reservationAggregate domainName

        let execute (command:BookADesk) reservationAggregate =
            {
                DeskBooked.ReservationId = ReservationAggregate.Id
                Date = command.Date
                EmailAddress = command.EmailAddress
                OfficeId = command.OfficeId
            }
            |> DeskBooked
            |> List.singleton

        let executeWith cmd reservationAggregate =
            (validate cmd reservationAggregate)
            |> Result.map (execute cmd)


        {
            ExecuteWith = executeWith
        }
