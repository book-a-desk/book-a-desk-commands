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
    let batch =  seq { yield events }
