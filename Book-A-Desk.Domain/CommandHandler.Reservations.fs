namespace Book_A_Desk.Domain.CommandHandler

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Commands

type BookADeskCommandHandler =
    {
        Handle: ReservationCommand -> Result<DomainEvent list,string>
    }

module BookADeskCommandHandler =
   let provide (eventStore:EventStore) =

       let handle (command : ReservationCommand) =
            let storeEventsForBatch aggregateId events =
                (aggregateId, events |> List.map ReservationEvent)
                |> List.singleton
                |> Map.ofList
                |> eventStore.AppendEvents
                |> Result.map (fun _ ->  events |> List.map ReservationEvent)

            let run executeCommandWith cmd (ReservationId aggregateId) =
                eventStore.GetEvents aggregateId
                |> List.map (function | ReservationEvent event -> event)
                |> ReservationAggregate.getCurrentStateFrom
                |> executeCommandWith cmd
                |> Result.bind (storeEventsForBatch aggregateId)

            match command with
            | BookADesk command ->
                let getOffices = fun () -> Offices.All
                let commandExecutor = // ToDo: use a reservationCommandFactory.CreateBookADeskReservationCommand ()
                    BookADeskReservationCommand.provide (BookADeskReservationValidator.validateCommand getOffices)
                run commandExecutor.ExecuteWith command ReservationAggregate.Id

       {
            Handle = handle
       }
