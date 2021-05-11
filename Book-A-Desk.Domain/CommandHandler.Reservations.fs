namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Events
open Book_A_Desk.Domain.Reservation.Commands.Instructions.Reservation

type ReservationsCommandHandler =
    {
        Handle: ReservationCommand -> Result<unit,string>
    }
    
module ReservationsCommandHandlerUsingInstructions =
    let handle (command : ReservationCommand) = reservationInstructions {
           match command with
           | BookADesk command ->
               let! offices = getOffices
               let! reservationAggregate = getReservationAggregate (ReservationAggregate.Id)
               let isValid = BookADeskReservationValidator.validateCommand offices command reservationAggregate
               
               match isValid with
               | Ok _ ->
                   let eventToAppend = 
                        {
                            DeskBooked.ReservationId = ReservationAggregate.Id
                            Date = command.Date
                            EmailAddress = command.EmailAddress
                            OfficeId = command.OfficeId
                        }
                        |> DeskBooked
                        |> ReservationEvent
                        |> List.singleton
                   do! appendEvents eventToAppend
               | Error _ -> ()
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
                () |> Ok
                //storeEventsForBatch aggregateId commandResult

            match command with
            | BookADesk command ->
                let commandExecutor = reservationCommandsFactory.CreateBookADeskCommand ()
                run commandExecutor.ExecuteWith command ReservationAggregate.Id

       {
            Handle = handle
       }
