namespace Book_A_Desk.Infrastructure

open Amazon.DynamoDBv2
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Domain
open FSharp.AWS.DynamoDB
open FSharp.Control

module rec DynamoDbEventStore =
    
    type DynamoDbEventStore =
        {
            GetReservationAggregateById: ReservationId -> Result<ReservationAggregate seq, string> Async
            GetReservationAggregate: ReservationCommand -> Result<ReservationAggregate seq, string> Async
            SaveReservationAggregate: ReservationAggregate -> unit Async
        }

    let provide (dynamoDbClient : IAmazonDynamoDB) =
        let table =
            TableContext
                .Create<ReservationAggregate>(
                    dynamoDbClient,
                    tableName = "ReservationAggregates",
                    createIfNotExists = false)

        {
            GetReservationAggregateById = getReservationAggregateById table
            GetReservationAggregate = getReservationAggregate table
            SaveReservationAggregate = saveReservationAggregate table
        }

    let private getReservationAggregateById table aggregateId = async {
        let! result = table.QueryAsync(keyCondition = <@ fun (r : ReservationAggregate) -> r.Id = aggregateId @>)

        return Result.Ok(result)
    }

    let private getReservationAggregate table reservationCommand = async {
        let! results = table.QueryAsync(keyCondition = <@ fun (r : ReservationAggregate) ->
            r.ReservationEvents
            |>

                = reservationCommand @>)

        return Result.Ok(results)
    }

    let private saveReservationAggregate table reservationAggregate = async {
        do! table.UpdateItemAsync(reservationAggregate) |> Async.Ignore
    }
