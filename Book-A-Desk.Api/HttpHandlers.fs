module BookADesk.Api.HttpHandlers

open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open BookADesk.Api.Models

let handleGetHello =
    fun (next : HttpFunc) (ctx : HttpContext) ->
        task {
            let response = {
                Text = "Hello, world!"
            }
            return! json response next ctx
        }
