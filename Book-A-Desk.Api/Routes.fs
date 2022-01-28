namespace Book_A_Desk.Api

open Book_A_Desk.Domain.Office.Domain
open Giraffe

open Book_A_Desk.Api.Models

type Routes =
    {
        HttpHandlers: HttpHandler
    }
module Routes =
    let provide (apiDependencyFactory:ApiDependencyFactory) =
        let httpHandlers : HttpHandler =
            choose [
                GET >=> choose [
                    route "/offices" >=> (
                        apiDependencyFactory.CreateOfficesHttpHandler ()
                        |> fun h -> h.HandleGetAll ()
                    )
                    routef "/offices/%O/availabilities" (
                        apiDependencyFactory.CreateOfficesHttpHandler ()
                        |> fun h -> h.HandleGetByDate
                    )
                ]
                POST >=> choose [
                    route "/bookings" >=> JsonBodyValidator.parseBody<Booking> (
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




