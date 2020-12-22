namespace Book_A_Desk.Common

[<AutoOpen>]
module ResultBuilder =
    type ResultBuilder() =        
        member _.ReturnFrom(m) = m
        member _.Return(x) = Ok x
        member _.Bind(m, f) = Result.bind f m
        
    let result = ResultBuilder()    
    