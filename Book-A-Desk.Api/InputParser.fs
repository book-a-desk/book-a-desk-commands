namespace Book_A_Desk.Api

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Giraffe
open Microsoft.AspNetCore.Http
open Book_A_Desk.Core

module InputParser =
    let parseDateFromContext (context : HttpContext) =
        context.TryGetQueryStringValue "date"
        |> Option.bind DateTime.TryParseOption
        
    let parseEmailFromContext (context : HttpContext) =
        let email =
            match context.TryGetQueryStringValue "email" with
            | None -> None
            | Some email ->
                let emailAddress = email |> EmailAddress
                Some emailAddress
        email
        
    let parseOfficeId (reference: string) =
        let wasParsed, officeIdGuid = Guid.TryParse reference
        
        if wasParsed then
            let officeId = officeIdGuid |> OfficeId
            Some officeId
        else
            None

    let parseOfficeIdFromContext (context : HttpContext) =
        context.TryGetQueryStringValue "office"
        |> Option.bind (fun (office: string) ->
            parseOfficeId office)