module BookADesk.Api.Routes

open Giraffe
open BookADesk.Api.HttpHandlers

let routes : HttpHandler =
    choose [
        POST >=> choose [
            route "/book-a-desk" >=> handleBookADesk 
        ]
        RequestErrors.NOT_FOUND "Not Found"
    ]
