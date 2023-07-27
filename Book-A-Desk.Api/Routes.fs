namespace Book_A_Desk.Api

open FSharp.Control.Tasks.V2.ContextInsensitive
open Giraffe
open Microsoft.AspNetCore.Http
open System.Threading.Tasks

open Book_A_Desk.Api.Models

type Routes =
    {
        HttpHandlers: HttpHandler
    }
    
type TokenValidationResult =
| ValidToken
| InvalidToken of string
| ConnectionError of string
    
module Routes =
    let private failAuthorization message next context =
        RequestErrors.unauthorized "Bearer" "BookADesk" (text message) next context
    let private internalServerError message next context =
        ServerErrors.internalError (text message) next context
    
    let private authorize
        (validateToken : string -> Task<TokenValidationResult>)
        : HttpHandler
        = fun next (context : HttpContext) -> task {
        
        let bearerToken =
            context.GetRequestHeader("Authorization")
        
        match bearerToken with
        | Ok bearerToken ->
            let bearerToken = bearerToken.Replace("Bearer ", "")
            match! validateToken bearerToken with
            | ValidToken ->
                printfn "ValidToken"
                return! next context
            | InvalidToken message ->
                printfn $"InvalidToken {message}"
                return! failAuthorization message next context
            | ConnectionError message ->
                printfn $"ConnectionError {message}"
                return! internalServerError message next context
        | Error e ->
            return! failAuthorization $"Could not get bearer token: {e}" next context
    }

    let provide (apiDependencyFactory:ApiDependencyFactory) validateToken =
        let httpHandlers : HttpHandler =
            choose [
                GET >=> choose [
                    route "/offices" >=>
                    (authorize validateToken) >=>
                    (
                        apiDependencyFactory.CreateOfficesHttpHandler ()
                        |> fun h -> h.HandleGetAll ()
                    )
                    routef "/offices/%O/availabilities" (
                        fun officeId ->
                            (authorize validateToken) >=>
                            (
                                apiDependencyFactory.CreateOfficesHttpHandler ()
                                |> fun h -> (h.HandleGetByDate officeId)
                            ))
                ]
                POST >=> choose [
                    route "/bookings" >=>
                    (authorize validateToken) >=>
                    JsonBodyValidator.parseBody<Booking> (
                        apiDependencyFactory.CreateBookingsHttpHandler ()
                        |> fun h -> h.HandlePostWith
                    )
                ]
                GET >=> choose [
                    route "/bookings" >=>  (
                        apiDependencyFactory.CreateBookingsHttpHandler ()
                        |> fun h -> h.HandleGet ()
                    )
                ]
                GET >=> choose [ 
                    route "/health"
                        >=> HealthHttpHandler.handle
                ]
                GET >=> choose [ 
                    route "/flags" >=> (
                        apiDependencyFactory.CreateFeatureFlagsHttpHandler ()
                        |> fun h -> h.HandleGetAll ()
                    )
                ]
                POST >=> choose [
                    route "/cancelBookings" >=> JsonBodyValidator.parseBody<Cancellation> (
                        apiDependencyFactory.CreateCancelBookingsHttpHandler ()
                        |> fun h -> h.HandlePostWith
                        )
                ]
                POST >=> choose [
                    route "/notify-office-restrictions" >=> JsonBodyValidator.parseBody<RestrictionNotifier> (
                        apiDependencyFactory.CreateNotifierHttpHandler ()
                        |> fun h -> h.HandlePostWith
                    )
                ]
                RequestErrors.NOT_FOUND "Not Found"
            ]

        {
            HttpHandlers = httpHandlers
        }




