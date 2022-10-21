namespace Book_A_Desk.Infrastructure

open System
open System.Text.Json
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events
open Book_A_Desk.Infrastructure

module rec DomainMapper =
    let toDomainByAggregateId aggregateId (infraEvents: ReservationEvent seq) =
        {
            ReservationAggregate.Id = aggregateId |> ReservationId
            ReservationEvents =
                Seq.map toDomainSingle infraEvents
                |> Seq.toList
        }

    let toDomain (infraEvents: ReservationEvent seq) =
        infraEvents
        |> Seq.groupBy (fun event -> event.AggregateId)
        |> Seq.map(
            fun (aggregateId, events) ->
            {
                ReservationAggregate.Id = aggregateId |> ReservationId
                ReservationEvents =
                    Seq.map toDomainSingle events
                    |> Seq.toList
            }
        )

    let toDomainSingle (infraEvent : ReservationEvent) =
        match infraEvent.ReservationType with
        | ReservationType.DeskBookedType ->
            let deskBooked = JsonSerializer.Deserialize<DeskBooked> infraEvent.Event
            {
                Date = deskBooked.Date.Date
                EmailAddress = deskBooked.EmailAddress
                OfficeId = deskBooked.OfficeId
            } : DeskBooked
            |> DeskBooked
        | ReservationType.DeskCancelledType ->
            let deskCancelled = JsonSerializer.Deserialize<DeskCancelled> infraEvent.Event
            {
                Date = deskCancelled.Date.Date
                EmailAddress = deskCancelled.EmailAddress
                OfficeId = deskCancelled.OfficeId
            } : DeskCancelled
            |> DeskCancelled
    
    let toInfra (reservationAggregate: ReservationAggregate) =
        let (ReservationId reservationId) = reservationAggregate.Id

        Seq.map toInfraSingle reservationAggregate.ReservationEvents
        |> Seq.map (fun reservationEvent ->
                {
                    reservationEvent with
                        AggregateId = reservationId
                }
            )

    let private toInfraSingle (reservationEvent: Book_A_Desk.Domain.Reservation.Events.ReservationEvent) =
        match reservationEvent with
        | DeskBooked deskBooked ->
            let infraDeskBooked =
                {
                    Date = deskBooked.Date
                    EmailAddress = deskBooked.EmailAddress
                    OfficeId = deskBooked.OfficeId
                }
            {
                AggregateId = Guid.Empty
                ReservationType = ReservationType.DeskBookedType
                Event =  JsonSerializer.Serialize infraDeskBooked
            }
        | DeskCancelled deskCancelled ->
            let infraDeskCancelled =
                {
                    Date = deskCancelled.Date
                    EmailAddress = deskCancelled.EmailAddress
                    OfficeId = deskCancelled.OfficeId
                }
            {
                AggregateId = Guid.Empty
                ReservationType = ReservationType.DeskCancelledType
                Event = JsonSerializer.Serialize infraDeskCancelled
            }