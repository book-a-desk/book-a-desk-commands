namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events
open Book_A_Desk.Domain.Reservation.Commands
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
               let isValid = BookADeskReservationValidator.validateCommand (fun () -> offices) command reservationAggregate
               
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
   let provide (eventStore:EventStore) reservationCommandsFactory =

       let handle (command : ReservationCommand) =
            let storeEventsForBatch aggregateId events =
                (aggregateId, events |> List.map ReservationEvent)
                |> List.singleton
                |> Map.ofList
                |> eventStore.AppendEvents

            let run executeCommandWith cmd (ReservationId aggregateId) = result {
                let! events = eventStore.GetEvents aggregateId
                let! commandResult =
                    events
                    |> List.map (function | ReservationEvent event -> event)
                    |> ReservationAggregate.getCurrentStateFrom
                    |> executeCommandWith cmd
                return storeEventsForBatch aggregateId commandResult
            }

            match command with
            | BookADesk command ->
                let commandExecutor = reservationCommandsFactory.CreateBookADeskCommand ()
                run commandExecutor.ExecuteWith command ReservationAggregate.Id

       {
            Handle = handle
       }
