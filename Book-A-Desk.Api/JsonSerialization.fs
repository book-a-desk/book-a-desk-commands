namespace Book_A_Desk.Api

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Newtonsoft.Json

module JsonBodyValidator =
    let parseBody<'T> handler: HttpHandler = fun next context ->
        task {
            try
                let! requestBody = context.BindJsonAsync<'T>()

                match box requestBody with
                | null ->
                    context.SetStatusCode 400
                    return! text "Body is empty" next context

                | _ ->
                    return! handler requestBody next context

            with
            | :? JsonReaderException ->
                context.SetStatusCode 400
                return! text "Invalid body" next context
        }
