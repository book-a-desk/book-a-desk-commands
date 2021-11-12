namespace Book_A_Desk.Api

open System
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Queries

open FSharp.Control.Tasks.V2.ContextInsensitive

type OfficeRestrictionNotifier =
    {
        Execute: DateTime -> Async<unit>
    }

module rec OfficeRestrictionNotifier =
    let provide notifyRestriction eventStore getOffices =
        
        let notifyRestrictionForBooking notifyRestriction (booking:Models.Booking) = task {
            let sent = notifyRestriction booking
            match sent with
            | Ok _ ->
                printfn $"Restriction notification message sent for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()}"
            | Error e ->
                printfn $"Error sending restriction notification error for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()} because {e}"               
        }
        
        let getEventsByDateFromEventStore eventStore date officeId = {
            let (ReservationId aggregateId) = ReservationAggregate.Id
            let! bookingEvents = eventStore.GetEvents aggregateId
            let getBookingsForDate = ReservationsQueriesHandler.get bookingEvents
            let result = getBookingsForDate date
            match result with
            | Ok _ ->
                printfn $"Restriction notification message sent for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()}"
            | Error e ->
                printfn $"Error sending restriction notification error for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()} because {e}"            
        }    
                
        let notifyOfficeBookings officeId day = async {
            let result = getEventsByDateFromEventStore eventStore day officeId                                                               
            match result with
            | Ok bookings ->
                bookings
                |> List.iter (fun (booking: Models.Booking) -> notifyRestrictionForBooking notifyRestriction booking)
            | Error e ->
                printfn "Internal Error: " + e
        }
        
        let execute (day:DateTime) = async {
            getOffices
            |> List.iter(fun office -> notifyOfficeBookings office.Id day)
        }
        {
            Execute = execute
        }
