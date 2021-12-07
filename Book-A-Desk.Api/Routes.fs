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
    
module Routes =
    let private failAuthorization message context =
        let failHandler = (text message >=> setStatusCode 401)
        failHandler earlyReturn context
    
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
                return! next context
            | InvalidToken message ->
                return! failAuthorization message context
        | Error e ->
            return! failAuthorization $"Could not get bearer token: {e}" context
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
                RequestErrors.NOT_FOUND "Not Found"
            ]

        {
            HttpHandlers = httpHandlers
        }




