namespace Book_A_Desk.Core

open FsToolkit.ErrorHandling

module AsyncResultExtension =
    let either ifOk ifError x = async {
        let! value = x
        return! Result.either ifOk ifError value
    }

    let sequential (asyncs: seq<Async<Result<'t, 'error>>>) : Async<Result<'t list, 'error>> =
        let rec loop acc remaining =
            asyncResult {
                match remaining with
                | [] -> return List.rev acc
                | x::xs ->
                    let! res = x: Async<Result<'t, 'error>>
                    return! loop (res::acc) xs
            }
        loop [] (List.ofSeq asyncs)

    let sequentialIter (asyncs: Async<Result<unit, 'error>> seq) : Async<Result<unit, 'error>> =
        let rec loop (remaining : Async<Result<unit, 'error>> list) =
            asyncResult {
                match remaining with
                | [] -> return ()
                | x::xs ->
                    do! x
                    return! loop xs
            }
        loop (List.ofSeq asyncs)

