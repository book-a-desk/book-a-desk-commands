namespace Book_A_Desk.Infrastructure

open Amazon.DynamoDBv2
open FSharp.AWS.DynamoDB
open FSharp.Control
open System
open Book_A_Desk.Domain.Events

type EventStore =
    {
        GetEvents: Guid -> Result<DomainEvent seq, string> Async
        AppendEvents: Map<Guid, DomainEvent seq> -> unit Async
    }

module rec DynamoDbEventStore =
    let provide (dynamoDbClient : IAmazonDynamoDB) =
        let table = TableContext.Create<DeskBooked>(dynamoDbClient,
                                                    tableName = "ReservationEvent",
                                                    createIfNotExists = false)
        
        {
            GetEvents = getEvent table
            AppendEvents = appendEvent table
        }
        
    let private getEvent table aggregateId = async {
        let! results = table.QueryAsync(keyCondition = <@ fun (r : DeskBooked) -> r.AggregateId = aggregateId @>)
        
        let domainResults = DomainMapper.toDomain results
        
        return Result.Ok(domainResults)        
    }
    
    let private appendEvent table events = async {
        let infraEvents = Map.map (fun _ events -> DomainMapper.toInfra events) events
        
        let events = infraEvents
                   |> EventBatcher.batchEvents
                   |> AsyncSeq.ofSeq
        do! AsyncSeq.iterAsync (fun infraEvents -> table.BatchPutItemsAsync(infraEvents) |> Async.Ignore) events        
    }
        