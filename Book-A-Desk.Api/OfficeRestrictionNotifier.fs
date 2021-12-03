namespace Book_A_Desk.Api

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Queries
open FsToolkit.ErrorHandling
open Book_A_Desk.Infrastructure.DynamoDbEventStore

type OfficeRestrictionNotifier =
    {
        NotifyOfficeRestrictions: DynamoDbEventStore -> RestrictionNotifier -> Async<Result<unit,string>>
    }

module rec OfficeRestrictionNotifier =
    let provide notifyRestriction =

        let getEventsByDateFromEventStore eventStore date = asyncResult {
            let (ReservationId aggregateId) = ReservationAggregate.Id
            let! bookingEvents = eventStore.GetEvents aggregateId
            let getBookingsForDate = ReservationsQueriesHandler.get bookingEvents
            return! getBookingsForDate date
        }

        let notifyOfficeRestrictions eventStore (restrictionNotifier: RestrictionNotifier) = asyncResult {
            let! bookings = getEventsByDateFromEventStore eventStore restrictionNotifier.Date

            bookings
            |> List.where(fun (booking: Booking) -> booking.OfficeId.Equals(restrictionNotifier.OfficeId))
            |> List.map(fun (booking:Booking) -> Booking.value restrictionNotifier.OfficeId booking.Date booking.EmailAddress)
            |> List.iter (fun booking -> notifyRestriction booking)
        }

        {
            NotifyOfficeRestrictions = notifyOfficeRestrictions
        }
