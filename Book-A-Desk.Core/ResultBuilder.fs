namespace Book_A_Desk.Core

[<AutoOpen>]
module ResultBuilder =
    type ResultBuilder() =
        member _.ReturnFrom(m) = m
        member _.Return(x) = Ok x
        member _.Bind(m, f) = Result.bind f m
        member _.BindReturn(x, f) = Result.map f x
        member _.MergeSources(t1: Result<'T,'U>, t2: Result<'T1,'U>) =
            match t1,t2 with
            | Ok t1, Ok t2 -> Ok (t1, t2)
            | Error e, _ -> Error e
            | _, Error e -> Error e
        
    let result = ResultBuilder()    
    