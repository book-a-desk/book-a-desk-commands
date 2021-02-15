namespace Book_A_Desk.Api

open Book_A_Desk.Domain.Office.Queries
open Giraffe

open Book_A_Desk.Api.Models

type Routes =
    {
        HttpHandlers: HttpHandler
    }
module Routes =
    let provide eventStore getOffices =
        let handlers = HttpHandlers.initialize eventStore getOffices

        let httpHandlers : HttpHandler =
            choose [
                GET >=> choose [
                    route "/offices" >=> handlers.Offices.HandleGetAll ()
                    routef "/offices/%O/availabilities" handlers.Offices.HandleGetByDate
                ]
                POST >=> choose [
                    route "/bookings"
                                >=> JsonBodyValidator.parseBody<Booking>
                                    handlers.Bookings.HandlePostWith
                ]
                RequestErrors.NOT_FOUND "Not Found"
            ]

        {
            HttpHandlers = httpHandlers
        }




