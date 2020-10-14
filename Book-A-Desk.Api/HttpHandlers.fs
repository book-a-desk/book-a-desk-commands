module BookADesk.Api.HttpHandlers

open Giraffe

let handleBookADesk : HttpHandler =
    Successful.OK "Ok"
