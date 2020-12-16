namespace Book_A_Desk.Api


open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

type OfficesHttpHandler =
    {
        HandleGet: unit -> HttpHandler
    }
    
module OfficesHttpHandler =
    let initialize =
        let handleGet () = fun next context ->
            task {
                let offices = [||]
                return! json offices next context
            }
        {
            HandleGet = handleGet
        }