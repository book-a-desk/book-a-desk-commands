namespace Book_A_Desk.Api

open System
open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe

type FlagsHttpHandler =
    {
        HandleFlags: unit -> HttpHandler
    }

module rec FlagsHttpHandler =
    let handle next context = task {
         let bookingCancellation = Environment.GetEnvironmentVariable("BOOKING_CANCELLATION") |> bool.Parse
         return! Successful.OK bookingCancellation next context
     }
