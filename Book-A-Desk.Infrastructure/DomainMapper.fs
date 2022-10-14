namespace Book_A_Desk.Infrastructure

open System.Text.Json
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events

module rec DomainMapper =
    let toDomain (infraAggregate: ReservationAggregate) =
        infraAggregate.ReservationEvents
        |> List.map toDomainSingle
        
    let toDomainSingle (infraAggregate : ) =
        match infraAggregate.ReservationEvents.ReservationType with
        | ReservationType.DeskBooked ->
            let deskBooked = JsonSerializer.Deserialize<Book_A_Desk.Infrastructure.DeskBooked> infraEvent.Event
            {                    
                ReservationId = ReservationId infraEvent.AggregateId
                Date = deskBooked.Date.Date
                EmailAddress = EmailAddress deskBooked.EmailAddress
                OfficeId = OfficeId deskBooked.OfficeId          
            } : DeskBooked
            |> DeskBooked
            |> ReservationEvent
        | ReservationType.DeskCancelled ->
            let deskCancelled = JsonSerializer.Deserialize<Book_A_Desk.Infrastructure.DeskCancelled> infraEvent.Event
            {
                ReservationId = ReservationId infraEvent.AggregateId
                Date = deskCancelled.Date.Date
                EmailAddress = EmailAddress deskCancelled.EmailAddress
                OfficeId = OfficeId deskCancelled.OfficeId
            } : DeskCancelled
            |> DeskCancelled
            |> ReservationEvent
    
    let toInfra (domainEvents: DomainEvent seq) =
        Seq.map toInfraSingle domainEvents

    let private toInfraSingle (domainEvent: DomainEvent) =
        match domainEvent with
        | ReservationEvent reservationEvent ->
            match reservationEvent with
            | DeskBooked deskBooked ->
                let (ReservationId reservationId) = deskBooked.ReservationId
                let infraDeskBooked =
                    {
                        Date = deskBooked.Date
                        EmailAddress =
                            let (EmailAddress email) = deskBooked.EmailAddress
                            email
                        OfficeId =
                            let (OfficeId officeId) = deskBooked.OfficeId
                            officeId
                    }
                {
                    AggregateId = reservationId
                    ReservationType = ReservationType.DeskBooked
                    Event =  JsonSerializer.Serialize infraDeskBooked
                }
            | DeskCancelled deskCancelled ->
                let (ReservationId reservationId) = deskCancelled.ReservationId
                let infraDeskCancelled =
                    {
                        Date = deskCancelled.Date
                        EmailAddress =
                            let (EmailAddress email) = deskCancelled.EmailAddress
                            email
                        OfficeId =
                            let (OfficeId officeId) = deskCancelled.OfficeId
                            officeId
                    }
                {
                    AggregateId = reservationId
                    ReservationType = ReservationType.DeskCancelled
                    Event = JsonSerializer.Serialize infraDeskCancelled
                }