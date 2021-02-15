module Book_A_desk.Domain.Events.Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events
open Xunit

[<Fact>]
let ``GIVEN an aggregateId and events with an invalid aggregateId and map not empty, when reading them, reading fails`` () =
    let aPreviousAggregateId = Guid.NewGuid ()
    let aggregateId = Guid.NewGuid ()
    let events = (aPreviousAggregateId, List.empty, Map.empty) |||> Map.add
    
    let result = EventsReader.getEvents aggregateId events
    match result with
    | Ok _ -> failwith "should have failed"
    | Error _ -> ()
    
[<Fact>]
let ``GIVEN an aggregateId and events with an empty map, when reading them, returns an empty list`` () =
    let aggregateId = Guid.NewGuid ()
    let events = Map.empty
    
    let result = EventsReader.getEvents aggregateId events
    match result with
    | Ok [] -> ()
    | Error _ -> failwith "should not have failed"
    | Ok _ -> failwith "should not return something else"
    
[<Fact>]
let ``GIVEN an aggregateId and events with a valid aggregateId, when reading them, reading succeeds`` () =
    let aggregateId = Guid.NewGuid ()
    let events = (aggregateId, List.empty, Map.empty) |||> Map.add
    
    let result = EventsReader.getEvents aggregateId events
    match result with
    | Ok _ -> ()
    | Error _ -> failwith "should have failed"
    
[<Fact>]
let ``GIVEN an aggregateId and events with a valid aggregateId, when reading them, gets events`` () =
    let aggregateId = Guid.NewGuid ()
    let eventForAggregate =
        {
            ReservationId = Guid.NewGuid () |> ReservationId
            Date = DateTime.MaxValue
            EmailAddress = EmailAddress "emailAddress"
            OfficeId = Guid.NewGuid () |> OfficeId
        }
        |> DeskBooked
        |> ReservationEvent
    let events = (aggregateId, [eventForAggregate], Map.empty) |||> Map.add
    
    let result = EventsReader.getEvents aggregateId events
    match result with
    | Ok eventsResult -> Assert.True(List.contains eventForAggregate eventsResult)
    | Error _ -> failwith "should have failed"
 
    
[<Fact>]
let ``GIVEN previous and new events with a new aggregateId, when appending, new aggregate is appended`` () =
    let aggregateId = Guid.NewGuid ()
    let previousEvents = (aggregateId, List.empty, Map.empty) |||> Map.add
    
    let newAggregateId = Guid.NewGuid ()
    let newEvents = (newAggregateId, List.empty, Map.empty) |||> Map.add
    
    let result = EventsWriter.appendEvents previousEvents newEvents
    
    Assert.True(Map.containsKey newAggregateId result)
    
[<Fact>]
let ``GIVEN previous and new events, when appending, events are appended`` () =
    let aggregateId = Guid.NewGuid ()
    let previousEvents = Map.empty
    
    let eventForAggregate =
        {
            ReservationId = Guid.NewGuid () |> ReservationId
            Date = DateTime.MaxValue
            EmailAddress = EmailAddress "emailAddress"
            OfficeId = Guid.NewGuid () |> OfficeId
        }
        |> DeskBooked
        |> ReservationEvent
    let newEvents = (aggregateId, [eventForAggregate], Map.empty) |||> Map.add
    
    let result = EventsWriter.appendEvents previousEvents newEvents
    let eventsResult = result.[aggregateId]
    
    Assert.True(List.contains eventForAggregate eventsResult)
    
[<Fact>]
let ``GIVEN previous and new events, when appending, previous events are not overwritten`` () =
    let aggregateId = Guid.NewGuid ()    
    let previousEvent =
        {
            ReservationId = Guid.NewGuid () |> ReservationId
            Date = DateTime.MaxValue
            EmailAddress = EmailAddress "anEmail"
            OfficeId = Guid.NewGuid () |> OfficeId
        }
        |> DeskBooked
        |> ReservationEvent
    
    let previousEvents =  (aggregateId, [previousEvent], Map.empty) |||> Map.add
    
    let eventForAggregate =
        {
            ReservationId = Guid.NewGuid () |> ReservationId
            Date = DateTime.MaxValue
            EmailAddress = EmailAddress "emailAddress"
            OfficeId = Guid.NewGuid () |> OfficeId
        }
        |> DeskBooked
        |> ReservationEvent
    let newEvents = (aggregateId, [eventForAggregate], Map.empty) |||> Map.add
    
    let result = EventsWriter.appendEvents previousEvents newEvents
    let eventsResult = result.[aggregateId]
    
    Assert.True(List.contains previousEvent eventsResult)