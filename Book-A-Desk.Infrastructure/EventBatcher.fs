namespace Book_A_Desk.Infrastructure

module rec EventBatcher =
    let rec batchEvents events =
        match Seq.length events with
            | length when length > 25 ->
                let first25 = Seq.take 25 events
                let nextBatches = batchEvents (Seq.skip 25 events)
                seq { yield first25; yield! nextBatches } 
            | _ ->
                Seq.singleton events