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
    
let generateDeskBooked n =
    
    
[<Fact>]
let ``Given An EventBatcher When Batching More Than 25 Events Returns Multiple Batches`` () =
    let deskBooked =
        {
            
        }