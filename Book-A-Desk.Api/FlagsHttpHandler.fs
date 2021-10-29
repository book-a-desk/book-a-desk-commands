namespace Book_A_Desk.Api

open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

type FlagsHttpHandler =
    {
        HandleGetAll: unit -> HttpHandler
    }

module rec FlagsHttpHandler =
    let initialize getFeatureFlags =
        {
            HandleGetAll = fun () -> handleGetAll getFeatureFlags 
        }
    let handleGetAll getFeatureFlags = fun next context ->
         task {
             return! Successful.OK getFeatureFlags next context
     }
