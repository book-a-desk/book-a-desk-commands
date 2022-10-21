namespace Book_A_Desk.Infrastructure

open Amazon.DynamoDBv2
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open FSharp.AWS.DynamoDB
open FSharp.Control

module rec DynamoDbEventStore =
    
    type DynamoDbEventStore =
        {
            GetEvents: Result<ReservationAggregate seq, string> Async
            GetReservationAggregateById: ReservationId -> Result<ReservationAggregate, string> Async
            SaveReservationAggregate: ReservationAggregate -> unit Async
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
            GetReservationAggregateById = getReservationAggregateById table
            SaveReservationAggregate = saveReservationAggregate table
        }

    let private getEvents table = async {
        let! results = table.QueryAsync(keyCondition = <@ fun (r : ReservationEvent) -> 1=1 @>)

        let domainResults = DomainMapper.toDomain results

        return Result.Ok(domainResults)
    }

    let private getReservationAggregateById table (ReservationId aggregateId) = async {
        let! results = table.QueryAsync(keyCondition = <@ fun (r : ReservationEvent) -> r.AggregateId = aggregateId @>)

        let domainResult = DomainMapper.toDomainByAggregateId aggregateId results
        return Result.Ok(domainResult)
    }

    let private saveReservationAggregate table reservationAggregate = async {
        let infraEvents = Map.map (fun _ reservationAggregate -> DomainMapper.toInfra reservationAggregate) (reservationAggregate.ReservationEvents |> List.toSeq)

        let events = infraEvents
                   |> EventBatcher.batchEvents
                   |> AsyncSeq.ofSeq
        do! AsyncSeq.iterAsync (fun infraEvents -> table.BatchPutItemsAsync(infraEvents) |> Async.Ignore) events
   }