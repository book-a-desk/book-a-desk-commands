module Tests

open System
open Book_A_Desk.Infrastructure
open Xunit

[<Fact>]
let ``Given An EventBatcher When Batching An Empty List Returns Empty List`` () =
    let result = EventBatcher.batchEvents Map.empty 
    Assert.Equal(0, Seq.length result)
    
[<Fact>]
let ``Given An EventBatcher When Batching Less Than 25 Events Returns A Single Batch``() =
    let deskBooked =
        {
            AggregateId = Guid.NewGuid()
            Date = DateTime.MaxValue
            EmailAddress = "test@test.com"
            OfficeId = Guid.NewGuid()
        }
    let batch = Map.empty.Add(Guid.NewGuid(), Seq.singleton deskBooked)
    let result = EventBatcher.batchEvents batch
    
    Assert.Single(result)
    
let generateDeskBooked () =
    Seq.initInfinite (fun i ->
        {
            AggregateId = Guid.NewGuid()
            Date = DateTime.MaxValue
            EmailAddress = $"test_{i}@test.com"
            OfficeId = Guid.NewGuid()
        })
    
[<Fact>]
let ``Given An EventBatcher When Batching More Than 25 Events Returns Multiple Batches`` () =
    let desksBooked = generateDeskBooked () |> Seq.take 30
    let batch = Map.empty.Add(Guid.NewGuid(), desksBooked)
    let result = EventBatcher.batchEvents batch
    
    Assert.Equal(2, Seq.length result)
    Assert.Equal(25, Seq.length(Seq.item 0 result))
    Assert.Equal(5, Seq.length(Seq.item 1 result))