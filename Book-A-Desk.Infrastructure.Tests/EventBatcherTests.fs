module Tests

open System
open Book_A_Desk.Infrastructure
open Xunit
    
let generateDeskBooked () =
    Seq.initInfinite (fun i ->
        {
            AggregateId = Guid.NewGuid()
            Date = DateTime.MaxValue
            EmailAddress = $"test_{i}@test.com"
            OfficeId = Guid.NewGuid()
        })
    
[<Fact>]
let ``Given An EventBatcher When Batching An Empty List Returns Empty List`` () =
    let result = EventBatcher.batchEvents Map.empty 
    Assert.Equal(0, Seq.length result)
    
[<Fact>]
let ``Given An EventBatcher When Batching Less Than 25 Events Returns A Single Batch``() =
    let batch = Map.empty.Add(Guid.NewGuid(), generateDeskBooked () |> Seq.take 1)
    let result = EventBatcher.batchEvents batch
    
    let resultBatch = Assert.Single(result)
    Assert.Single(resultBatch)
    

[<Fact>]
let ``Given An EventBatcher When Batching More Than 25 Events Returns Multiple Batches`` () =
    let desksBooked = generateDeskBooked () |> Seq.take 30
    let batch = Map.empty.Add(Guid.NewGuid(), desksBooked)
    let result = EventBatcher.batchEvents batch
    
    Assert.Equal(2, Seq.length result)
    Assert.Equal(25, Seq.length(Seq.item 0 result))
    Assert.Equal(5, Seq.length(Seq.item 1 result))
    
[<Fact>]
let ``Given an EventBatcher When Batching Multiple Aggregates Returns Multiple Batches`` () =
    let desksBooked1 = generateDeskBooked () |> Seq.take 5
    let desksBooked2 = generateDeskBooked () |> Seq.take 7
    let batch = Map.empty
                   .Add(Guid.NewGuid(), desksBooked1)
                   .Add(Guid.NewGuid(), desksBooked2)
    let result = EventBatcher.batchEvents batch

    Assert.Equal(2, Seq.length result)
    Assert.Equal(7, Seq.length(Seq.item 0 result))
    Assert.Equal(5, Seq.length(Seq.item 1 result))