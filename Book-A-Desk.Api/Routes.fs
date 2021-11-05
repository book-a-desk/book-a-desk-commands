namespace Book_A_Desk.Api

open Giraffe

open Book_A_Desk.Api.Models
open Okta.AspNetCore

type Routes =
    {
        HttpHandlers: HttpHandler
    }
module Routes =
    let private authorize : HttpHandler =
        requiresAuthentication (challenge OktaDefaults.ApiAuthenticationScheme)
    
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




