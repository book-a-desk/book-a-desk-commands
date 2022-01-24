namespace Book_A_Desk.Api

open Book_A_Desk.Api.Models
open Book_A_Desk.Core
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Queries
open FsToolkit.ErrorHandling
open Book_A_Desk.Infrastructure.DynamoDbEventStore

type OfficeRestrictionNotifier =
    {
        NotifyOfficeRestrictions: DynamoDbEventStore -> RestrictionNotifier -> Async<Result<unit list,string>>
    }

module rec OfficeRestrictionNotifier =
    let provide (notifyRestriction: Models.Booking -> Async<Result<unit, string>>) =

        let getEventsByDateFromEventStore eventStore date = asyncResult {
            let (ReservationId aggregateId) = ReservationAggregate.Id
            let! bookingEvents = eventStore.GetEvents aggregateId
            let getBookingsForDate = ReservationsQueriesHandler.get bookingEvents
            return! getBookingsForDate date
        }

        let notifyOfficeRestrictions eventStore (restrictionNotifier: RestrictionNotifier) = asyncResult {
            let! bookings = getEventsByDateFromEventStore eventStore restrictionNotifier.Date

            let! notifyBookings =
                bookings
                |> List.where(fun (booking: Booking) -> booking.OfficeId.Equals(restrictionNotifier.OfficeId))
                |> List.map(fun (booking:Booking) -> notifyRestriction (Booking.value restrictionNotifier.OfficeId booking.Date booking.EmailAddress))
                |> AyncResultExtension.sequential

            return notifyBookings
        }

        {
            NotifyOfficeRestrictions = notifyOfficeRestrictions
        }
