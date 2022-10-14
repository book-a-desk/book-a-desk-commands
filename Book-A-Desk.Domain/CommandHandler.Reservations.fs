namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Cancellation.Commands
open Book_A_Desk.Domain.Errors
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Commands

type ReservationsCommandHandler =
    {
        Handle: ReservationCommand -> Result<ReservationAggregate list,ReservationError>
    }
    
module ReservationsCommandHandler =  
   let provide events reservationCommandsFactory =       
       let handle (command : ReservationCommand) =
            let run executeCommandWith cmd =
                events
                |> List.map (function | ReservationEvent event -> event)
                |> ReservationAggregate.getCurrentStateFrom
                |> executeCommandWith cmd
                |> Result.map (List.map (function event -> ReservationEvent event))

            let reservationAggregate = getAggregateFrom command

            match command with
            | BookADesk command ->
                let commandExecutor = reservationCommandsFactory.CreateBookADeskCommand ()
                run commandExecutor.ExecuteWith command reservationAggregate
            | CancelBookADesk command ->
                let commandExecutor = reservationCommandsFactory.CreateCancelBookADeskCommand ()
                run commandExecutor.ExecuteWith command reservationAggregate

       {
            Handle = handle
       }
