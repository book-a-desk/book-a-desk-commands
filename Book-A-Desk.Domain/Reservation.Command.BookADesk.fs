namespace Book_A_Desk.Domain.Reservation.Commands

open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events

type BookADeskReservationCommand =
    {
        ExecuteWith: BookADesk -> ReservationAggregate option -> Result<ReservationEvent list, string>
    }

module BookADeskReservationCommand =
    let provide validateCommand =
        let execute (command:BookADesk) =
            {
                DeskBooked.ReservationId = ReservationAggregate.Id
                Date = command.Date
                EmailAddress = command.EmailAddress
                OfficeId = command.OfficeId
            }
            |> DeskBooked
            |> List.singleton

        let executeWith cmd reservationAggregate =
            validateCommand cmd reservationAggregate
            |> Result.map (fun _ -> execute cmd)
            
        {
            ExecuteWith = executeWith
        }
