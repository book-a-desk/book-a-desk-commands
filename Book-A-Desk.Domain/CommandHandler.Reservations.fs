namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Events

type CommandResult =
    | Append of ReservationAggregate
    | DoNothing

type ReservationsCommandHandler =
    {
        Handle: ReservationCommand -> Result<CommandResult,string>
    }
    
module ReservationsCommandHandler =  
   let provide events reservationCommandsFactory =
       let handle (command : ReservationCommand) =
            let run executeCommandWith cmd (ReservationId aggregateId) =
                let commandResult =
                    events
                    |> List.map (function | ReservationEvent event -> event)
                    |> ReservationAggregate.getCurrentStateFrom
                    |> executeCommandWith cmd
                match commandResult with
                | Ok reservationEvents ->
                    Append reservationEvents
                | Error _ ->
                    DoNothing

            match command with
            | BookADesk command ->
                let commandExecutor = reservationCommandsFactory.CreateBookADeskCommand ()
                run commandExecutor.ExecuteWith command ReservationAggregate.Id

       {
            Handle = handle
       }
