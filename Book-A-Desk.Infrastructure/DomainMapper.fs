namespace Book_A_Desk.Infrastructure

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events

module rec DomainMapper =
    let toDomain (infraEvents: Book_A_Desk.Infrastructure.DeskBooked seq) =
        Seq.map toDomainSingle infraEvents
        
    let toDomainSingle (infraEvent : Book_A_Desk.Infrastructure.DeskBooked) =
        {                    
            ReservationId = ReservationId infraEvent.AggregateId
            Date = infraEvent.Date.Date
            EmailAddress = EmailAddress infraEvent.EmailAddress
            OfficeId = OfficeId infraEvent.OfficeId            
        }
        |> DeskBooked
        |> ReservationEvent
    
    let toInfra (domainEvents: DomainEvent seq) =
        Seq.map toInfraSingle domainEvents

    let private toInfraSingle (domainEvent: DomainEvent) =
        match domainEvent with
        | ReservationEvent reservationEvent ->
            match reservationEvent with
            | DeskBooked deskBooked ->
                let (ReservationId reservationId) = deskBooked.ReservationId
                let (EmailAddress email) = deskBooked.EmailAddress
                let (OfficeId officeId) = deskBooked.OfficeId
                {
                    AggregateId = reservationId
                    EventId = Guid.NewGuid()
                    Date = DateTimeOffset(deskBooked.Date)
                    EmailAddress = email
                    OfficeId = officeId
                }