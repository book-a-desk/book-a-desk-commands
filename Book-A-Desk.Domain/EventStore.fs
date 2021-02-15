namespace Book_A_Desk.Domain

open System

open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation.Events

type EventStore =
    {
        GetEvents: Guid -> Result<DomainEvent list, string>
        AppendEvents: Map<Guid, DomainEvent list> -> unit
    }

module EventsReader =
    let getEvents aggregateId events =
        match Map.tryFind aggregateId events with
        | Some events ->
            Result.Ok events
        | None ->
            match Map.count events with
            | 0 -> Result.Ok []
            | _ -> Result.Error "could not find aggregate id"
  
module EventsWriter =          
    let appendEvents previousEventsById newEventsById =
        newEventsById
        |> Map.fold (fun (appendedEvents : Map<Guid, DomainEvent list>) aggregateId newEvents ->
            match appendedEvents.TryFind aggregateId with
            | Some events -> appendedEvents.Add(aggregateId, List.concat [events; newEvents])
            | None -> appendedEvents.Add(aggregateId, newEvents)) previousEventsById

module InMemoryEventStore =    
    type private MailboxEventStore =
    | GetEvents of Guid * AsyncReplyChannel<Result<DomainEvent list, string>>
    | AppendEvents of Map<Guid, DomainEvent list>
    
    let private memoryStore =
        MailboxProcessor.Start(fun inbox ->
            let events = Map.empty
            
            let rec loop events = async {
                let! msg = inbox.Receive()
                
                let newEvents =
                    match msg with
                    | GetEvents (aggregateId, replyChannel) ->
                        replyChannel.Reply(EventsReader.getEvents aggregateId events)
                        events
                    | AppendEvents newEventsById ->
                        EventsWriter.appendEvents events newEventsById
                    
                return! loop newEvents
            }
            loop events)

    let provide () =
        let getEvents aggregateId =
            memoryStore.PostAndReply(fun rc -> (aggregateId, rc) |> GetEvents)

        let storeEvents mapOfEvents =
            memoryStore.Post(AppendEvents mapOfEvents)

        {
            GetEvents = getEvents
            AppendEvents = storeEvents
        }
