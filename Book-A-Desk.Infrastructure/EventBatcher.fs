namespace Book_A_Desk.Infrastructure

module rec EventBatcher =

    let batchEvents events =
        events
        |> Map.toSeq
        |> Seq.map snd
        |> Seq.map batch25Events
        |> Seq.concat
        
    let rec private batch25Events events =
        match Seq.length events with
            | length when length > 25 ->
                let first25 = Seq.take 25 events
                let nextBatches = batch25Events (Seq.skip 25 events)
                seq { yield first25; yield! nextBatches } 
            | _ ->
                Seq.singleton events