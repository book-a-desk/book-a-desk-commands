namespace Book_A_Desk.Infrastructure

open Amazon.DynamoDBv2
open FSharp.AWS.DynamoDB
open FSharp.Control
open Book_A_Desk.Domain.Events

module rec DynamoDbEventStore =
    
    type DynamoDbEventStore =
        {        
            GetEvents: unit -> Result<DomainEvent seq, string> Async
            AppendEvents: DomainEvent seq -> unit Async
        }
    
    let provide (dynamoDbClient : IAmazonDynamoDB) =
        let table =
            TableContext
                .Create<ReservationEvent>(
                    dynamoDbClient,
                    tableName = "ReservationEvents",
                    createIfNotExists = false)
        
        {
            GetEvents = getEvents table
            AppendEvents = appendEvent table
        }
        
    let private getEvents table () = async {
        let! results = table.ScanAsync()
        
        let domainResults = DomainMapper.toDomain results
        
        return Result.Ok(domainResults)
    }
    
    let private appendEvent table events = async {
        let infraEvents = DomainMapper.toInfra events
        
        let events = infraEvents
                   |> Seq.splitInto 25
                   |> AsyncSeq.ofSeq
        do! AsyncSeq.iterAsync (fun infraEvents -> table.BatchPutItemsAsync(infraEvents) |> Async.Ignore) events
    }