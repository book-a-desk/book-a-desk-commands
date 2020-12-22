namespace Book_A_Desk.Api

open Giraffe

open Book_A_Desk.Api.Models

type Routes =
    {
        HttpHandlers: HttpHandler
    }
module Routes =
    let provide eventStore =
        let handlers = HttpHandlers.initialize eventStore

        let httpHandlers : HttpHandler =
            choose [
                GET >=> choose [
                    route "/offices" >=> handlers.Offices.HandleGet ()
                ]
                POST >=> choose [
                    route "/bookings"
                                >=> JsonBodyValidator.parseBody<Booking>
                                (fun booking ->
                                    handlers.Bookings.HandlePostWith booking)
                ]
                RequestErrors.NOT_FOUND "Not Found"
            ]

        {
            HttpHandlers = httpHandlers
        }




