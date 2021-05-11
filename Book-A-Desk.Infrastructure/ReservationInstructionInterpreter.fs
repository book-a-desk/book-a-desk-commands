namespace Book_A_Desk.Infrastructure

open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands.Instructions.ReservationInstructions
open Book_A_Desk.Domain.Reservation.Domain

module ReservationInstructionInterpreter =
    
    // Note: To make the program run, all you need to do is:
    // ReservationsCommandHandlerUsingInstructions.handle command
    // |> ReservationInstructionInterpreter.interpret
    
    let rec interpret (eventStore : EventStore) program = async {
        let recurse = interpret eventStore
        
        match program with
        | Stop _ -> ()
        | Next (AppendEvents (events, next)) ->
            let (ReservationId id) = ReservationAggregate.Id
            do!
                [id, events]
                |> Map.ofList
                |>  eventStore.AppendEvents
            do! recurse next
        | Next (GetReservationAggregate (reservationId, next)) ->
            let (ReservationId id) = reservationId
            let! events = eventStore.GetEvents id
            match events with
            | Ok events ->
                let events = Seq.map (fun (ReservationEvent event) -> event) events
                let aggregate = ReservationAggregate.getCurrentStateFrom events
                let nextProgram = next aggregate
                do! recurse nextProgram
            | Error e ->
                failwith e
        | Next (GetOffices next) ->
            let offices = Offices.All
            let nextProgram = next offices
            do! recurse nextProgram
    }