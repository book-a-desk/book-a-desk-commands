namespace Book_A_Desk.Domain.Reservation

open System
open Book_A_Desk.Domain.Reservation.Domain

module ReservationAggregate =
    // As long as we don't have aggregates for offices and desk on their own we will operate only on one instance of this aggregate
    let Id = Guid.Parse("DCD544FB-8DB3-489E-805C-B6F41F32910D") |> ReservationId

