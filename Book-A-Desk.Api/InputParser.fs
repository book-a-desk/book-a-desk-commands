namespace Book_A_Desk.Api

open Giraffe
open Microsoft.AspNetCore.Http
open Book_A_Desk.Core

module InputParser =
    let parseDateFromContext (context : HttpContext) =
        context.TryGetQueryStringValue "date"
        |> Option.bind DateTime.TryParseOption