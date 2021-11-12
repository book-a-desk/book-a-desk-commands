namespace Book_A_Desk.Api

open System
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.QueriesHandler
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Queries
open FsToolkit.ErrorHandling
open Book_A_Desk.Infrastructure.DynamoDbEventStore
open FSharp.Control.Tasks.V2.ContextInsensitive

type OfficeRestrictionNotifier =
    {
        Execute: DateTime -> Async<unit>
    }

module rec OfficeRestrictionNotifier =
    let provide (notifyRestriction: Booking -> Async<Result<unit, string>>) eventStore (getOffices: unit -> Office list) =
        
        let notifyRestrictionForBooking (notifyRestriction: Models.Booking -> Async<Result<unit,string>>) (booking:Models.Booking) = task {
            let! sent = notifyRestriction booking
            match sent with
            | Ok _ -> ()
            | Error e ->
                printfn $"Error sending restriction notification error for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()} because {e}"
        }
        
        let getEventsByDateFromEventStore eventStore date = asyncResult {
            let (ReservationId aggregateId) = ReservationAggregate.Id
            let! bookingEvents = eventStore.GetEvents aggregateId
            let getBookingsForDate = ReservationsQueriesHandler.get bookingEvents
            return! getBookingsForDate date
        }    
                
        let notifyOfficeBookings officeId day = async {
            let! bookings = getEventsByDateFromEventStore eventStore day
            match bookings with
            | Ok bookings ->
                bookings
                |> List.filter(fun (booking: Booking) -> booking.OfficeId.Equals(officeId))
                |> List.map(fun (booking:Booking) -> Booking.value officeId booking.Date booking.EmailAddress)
                |> List.iter (fun (booking: Models.Booking) ->  notifyRestrictionForBooking notifyRestriction booking |> Async.AwaitTask |> Async.RunSynchronously)
            | Error e ->
                printfn $"Internal Error: %s{e}"
        }
        
        let execute (day:DateTime) = async {
            let result = OfficeQueriesHandler.getAll getOffices
            match result with
            | Ok offices ->
                offices
                |> List.iter(fun office -> notifyOfficeBookings office.Id day |> Async.RunSynchronously)
            | Error e ->
                printfn $"Internal Error: %s{e}"
        }

        {
            Execute = execute
        }
