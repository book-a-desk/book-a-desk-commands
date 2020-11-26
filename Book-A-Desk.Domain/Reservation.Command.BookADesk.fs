namespace Book_A_Desk.Domain.Reservation.Commands

open System

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events

type BookADeskReservationCommand =
    {
        ExecuteWith: BookADesk -> Result<ReservationEvent list, string>
    }

module BookADeskReservationCommand =
    let provide getValidationResultOf =
        let validate (command:BookADesk) reservationAggregate  = fun () ->
             if command.EmailAddress = EmailAddress "" then
                    Error "The e-mail address must not be empty."
                else if command.Date < DateTime.Now then
                    Error "Date must be greater than today."
                else if command.OfficeID <> OfficeID "Berlin" && command.OfficeID <> OfficeID "Montreal" then
                    Error "You must enter a valid office ID."
                else
                    Ok reservationAggregate

        let execute (command:BookADesk) reservationAggregate =
            {
                DeskBooked.ReservationId = ReservationAggregate.Id
                Date = command.Date
                EmailAddress = command.EmailAddress
                OfficeID = command.OfficeID
            }
            |> DeskBooked
            |> List.singleton

        let executeWith cmd =
            getValidationResultOf (validate cmd None)
            |> Result.map (execute cmd)


        {
            ExecuteWith = executeWith
        }
