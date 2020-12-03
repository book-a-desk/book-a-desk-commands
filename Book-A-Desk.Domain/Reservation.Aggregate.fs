namespace Book_A_Desk.Domain.Reservation

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events

type Booking =
    {
        OfficeId: OfficeId
        EmailAddress: EmailAddress
        Date: DateTime
    }

type ReservationAggregate =
    {
        Id: ReservationId
        BookedDesks: Booking list
    }

module ReservationAggregate =
    // As long as we don't have aggregates for offices and desk on their own we will operate only on one instance of this aggregate
    let Id = Guid.Parse("DCD544FB-8DB3-489E-805C-B6F41F32910D") |> ReservationId

    let applyEventTo reservation event =
        match reservation with
        | None ->
            {
                ReservationAggregate.Id = Id
                BookedDesks = []
            }
            |> Some
        | Some reservation ->
            match event with
            | DeskBooked event ->
                let booking =
                    {
                       OfficeId = event.OfficeId
                       EmailAddress = event.EmailAddress
                       Date = event.Date
                    }
                { reservation with BookedDesks = booking :: reservation.BookedDesks }
                |> Some


    let getCurrentStateFrom events =
        events
        |> Seq.fold applyEventTo None
