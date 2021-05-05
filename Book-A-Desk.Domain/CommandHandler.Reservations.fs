namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Commands

type ReservationsCommandHandler =
    {
        Handle: ReservationCommand -> Result<unit,string>
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
