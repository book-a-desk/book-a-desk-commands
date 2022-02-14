namespace Book_A_Desk.Api

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

type FlagsHttpHandler =
    {
        HandleGetAll: unit -> HttpHandler
    }

module rec FlagsHttpHandler =
    let initialize featureFlags =
        {
            HandleGetAll = fun () -> handleGetAll featureFlags 
        }
    let handleGetAll (featureFlags : FeatureFlags) = fun next context ->
         task {
             return! Successful.OK featureFlags next context
     }
