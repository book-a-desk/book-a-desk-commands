﻿namespace Book_A_Desk.Infrastructure

open Amazon.DynamoDBv2
open Amazon.DynamoDBv2.DataModel
open FSharp.AWS.DynamoDB
open FSharp.Control
open System
open Book_A_Desk.Domain.Events
open Book_A_Desk.Api

type EventStore =
    {
        GetEvents: Guid -> Result<DomainEvent seq, string> Async
        AppendEvents: Map<Guid, DomainEvent seq> -> unit Async
    }

module rec DynamoDbEventStore =
    let provide (dynamoDbClient : IAmazonDynamoDB) (config: DynamoDBConfiguration) =
        let table = TableContext.Create<DeskBooked>(dynamoDbClient,
                                                    tableName = config.ReservationTableName,
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
                       |> Map.toSeq
                       |> Seq.map snd
                       |> AsyncSeq.ofSeq
            do! AsyncSeq.iterAsync (fun infraEvents -> table.BatchPutItemsAsync(infraEvents) |> Async.Ignore) events        
    }
        