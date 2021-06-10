namespace Book_A_Desk.Api

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

type HealthHttpHandler =
    {
        HandleHealth: unit -> HttpHandler
    }

module rec HealthHttpHandler =
    let handle next context = task {
         return! Successful.OK "healthy" next context
     }
