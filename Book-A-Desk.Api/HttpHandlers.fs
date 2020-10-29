module BookADesk.Api.HttpHandlers

open Giraffe
open Microsoft.AspNetCore.Http
open FSharp.Control.Tasks.V2.ContextInsensitive
open Bookadesk.Commands.Domain

let handleBookADesk (next : HttpFunc) (ctx : HttpContext) =
    task {
        let! model = ctx.BindModelAsync<BookADesk>()
        let result = BookADeskCommandHandler.Handle model InMemoryEventStore.storeEvent
        match result with
        | Ok _ -> return! json model next ctx
        | Error e -> return! failwith e
    }