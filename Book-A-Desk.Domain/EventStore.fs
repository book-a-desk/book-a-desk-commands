namespace Book_A_Desk.Domain

open System

open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation.Events

type EventStore =
    {
        GetEvents: Guid -> DomainEvent list
        AppendEvents: Map<Guid, DomainEvent list> -> Result<unit, string>
    }

module InMemoryEventStore =
    open System.Collections.Generic

    let provide () =
        let store = Dictionary<Guid, List<DomainEvent>> ()

        let getEvents aggregateId =
            []

        let storeEvents mapOfEvents =

            try
                mapOfEvents
                |> Map.iter (fun aggregateId (events: DomainEvent list) -> store.[aggregateId].AddRange events )

                Ok ()
            with
            | _ -> Error "Error while storing event"


        {
            GetEvents = getEvents
            AppendEvents = storeEvents
        }
