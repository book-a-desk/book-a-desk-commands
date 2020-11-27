namespace Book_A_Desk.Domain.Reservation.Commands

open System

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events

type BookADeskReservationCommand =
    {
        ExecuteWith: BookADesk -> ReservationAggregate option -> Result<ReservationEvent list, string>
    }

module BookADeskReservationCommand =
    let provide getValidationResultOf =
        let validate (command:BookADesk) reservationAggregate  = fun () ->
             // Can/should be removed into a separate module
             let noBookableDesksAvailable reservationAggregate =
                 match reservationAggregate with
                 | None -> true
                 | Some reservationAggregate ->
                     let maxAllowedBookingsPerOffice =
                         Offices.All
                         |> List.find (fun x -> x.Id = command.OfficeId)
                         |> fun x -> x.BookableDesksPerDay

                     reservationAggregate.BookedDesks
                     |> List.filter (fun b -> b.Date = command.Date && b.OfficeId = command.OfficeId)
                     |> List.length
                        >= maxAllowedBookingsPerOffice

             if command.EmailAddress = EmailAddress "" then
                    Error "The e-mail address must not be empty."
                else if command.Date < DateTime.Now then
                    Error "Date must be greater than today."
                else if Offices.All |> List.exists (fun office -> office.Id = command.OfficeId) |> not then
                    Error "You must enter a valid office ID."
                else if noBookableDesksAvailable reservationAggregate then
                    Error (sprintf "The office is booked out at %s" (command.Date.ToShortDateString()))
                else
                    Ok reservationAggregate

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
            getValidationResultOf (validate cmd reservationAggregate)
            |> Result.map (execute cmd)


        {
            ExecuteWith = executeWith
        }
