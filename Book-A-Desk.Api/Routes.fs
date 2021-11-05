namespace Book_A_Desk.Api

open Giraffe

open Book_A_Desk.Api.Models
open Microsoft.AspNetCore.Http
open Okta.AspNetCore

type Routes =
    {
        HttpHandlers: HttpHandler
    }
module Routes =
    let private authorize : HttpHandler =
        let apiKey = "some-secret-key-1234" // where do we get the key from?

        let validateApiKey (ctx : HttpContext) =
            match ctx.TryGetRequestHeader "X-API-Key" with
            | Some key -> apiKey.Equals key
            | None     -> false
        let accessDenied   = setStatusCode 401 >=> text "Access Denied"
        let requiresApiKey =
            authorizeRequest validateApiKey accessDenied
        requiresApiKey

    let provide (apiDependencyFactory:ApiDependencyFactory) =
        let httpHandlers : HttpHandler =
            choose [
                GET >=> choose [
                    route "/offices" >=>
                    authorize >=>
                    (
                        apiDependencyFactory.CreateOfficesHttpHandler ()
                        |> fun h -> h.HandleGetAll ()
                    )
                    routef "/offices/%O/availabilities" (
                        fun officeId ->
                            authorize >=>
                            (
                                apiDependencyFactory.CreateOfficesHttpHandler ()
                                |> fun h -> (h.HandleGetByDate officeId)
                            ))
                ]
                POST >=> choose [
                    route "/bookings" >=>
                    authorize >=>
                    JsonBodyValidator.parseBody<Booking> (
                        apiDependencyFactory.CreateBookingsHttpHandler ()
                        |> fun h -> h.HandlePostWith
                    )
                ]
                GET >=> choose [ 
                    route "/health"
                        >=> HealthHttpHandler.handle
                ]
                RequestErrors.NOT_FOUND "Not Found"
            ]

        {
            HttpHandlers = httpHandlers
        }




