namespace Book_A_Desk.Api

open Giraffe
open Microsoft.AspNetCore.Http

// Domain-transfer object for problem+json types.
// Specified by standard RFC 7807
// https://tools.ietf.org/html/rfc7807
type ProblemDetailsDto =
    {
        Title: string
        Detail: string
    }


// Giraffe, but with a longer neck.
[<AutoOpen>]
module GiraffeExtensions =

    let jsonProblem<'T> (dataObj: 'T): HttpHandler =
        fun (next: HttpFunc) (ctx: HttpContext) ->
            ctx.SetContentType "application/problem+json"
            let serializer = ctx.GetJsonSerializer()
            serializer.SerializeToBytes dataObj
            |> ctx.WriteBytesAsync
