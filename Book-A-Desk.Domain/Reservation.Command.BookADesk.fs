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
    let private validateCommand (cmd : BookADesk) reservationAggregate =
        BookADeskReservationValidator.validateEmailIsNotEmpty cmd.EmailAddress
        |> Result.bind (fun _ -> BookADeskReservationValidator.validateDateIsInTheFuture cmd.Date)
        |> Result.bind (fun _ -> BookADeskReservationValidator.validateOfficeIdIsValid cmd.OfficeId)
        |> Result.bind (fun _ -> BookADeskReservationValidator.validateOfficeIsAvailable cmd.Date cmd.OfficeId reservationAggregate)
        
    let provide () =

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
