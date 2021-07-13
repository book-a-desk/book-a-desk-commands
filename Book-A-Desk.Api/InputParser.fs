namespace Book_A_Desk.Api

open System
open Book_A_Desk.Domain.Office.Domain
open Giraffe
open Microsoft.AspNetCore.Http
open Book_A_Desk.Core

module InputParser =
    let parseDateFromContext (context : HttpContext) =
        context.TryGetQueryStringValue "date"
        |> Option.bind DateTime.TryParseOption
        
    let parseOfficeId (reference: string) =
        let wasParsed, officeIdGuid = Guid.TryParse reference
        let officeId = officeIdGuid |> OfficeId        
        if wasParsed then Ok officeId
        else Error ()