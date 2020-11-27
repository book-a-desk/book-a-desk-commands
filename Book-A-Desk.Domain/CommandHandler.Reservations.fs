namespace Book_A_Desk.Domain.CommandHandler

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Commands

type BookADeskCommandHandler =
    {
        Handle: ReservationCommand -> Result<DomainEvent list,string>
    }

module BookADeskCommandHandler =
   let provide (eventStore:EventStore) =
       let getValidationResultOf = fun f -> f()

       let handle (command : ReservationCommand) =
            let storeEventsForBatch aggregateId events =
                (aggregateId, events |> List.map ReservationEvent)
                |> List.singleton
                |> Map.ofList
                |> eventStore.AppendEvents
                |> Result.map (fun _ ->  events |> List.map ReservationEvent)

            let run executeCommandWith cmd (ReservationId aggregateId) =
                eventStore.GetEvents aggregateId
                |> List.map (function | ReservationEvent event -> event | _ -> failwithf "There is an unexpected event type for AggregateId:%s" (aggregateId.ToString()))
                |> ReservationAggregate.getCurrentStateFrom
                |> executeCommandWith cmd
                |> Result.bind (storeEventsForBatch aggregateId)

            match command with
            | BookADesk command ->
                let commandExecutor = // ToDo: use a reservationCommandFactory.CreateBookADeskReservationCommand ()
                    BookADeskReservationCommand.provide getValidationResultOf
                run commandExecutor.ExecuteWith command ReservationAggregate.Id
            | _ -> failwith " ........"


       {
            Handle = handle
       }
