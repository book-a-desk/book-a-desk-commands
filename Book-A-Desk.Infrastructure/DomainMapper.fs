namespace Book_A_Desk.Infrastructure

open System
open System.Text.Json
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events

module rec DomainMapper =
    // Todo: add support for other events
    let toDomain (infraEvents: Book_A_Desk.Infrastructure.ReservationEvent seq) =
        Seq.map toDomainSingle infraEvents
        
    let toDomainSingle (infraEvent : Book_A_Desk.Infrastructure.ReservationEvent) =
        let deskBooked = JsonSerializer.Deserialize<Book_A_Desk.Infrastructure.DeskBooked> infraEvent.Event
        {                    
            ReservationId = ReservationId infraEvent.AggregateId
            Date = deskBooked.Date.Date
            EmailAddress = EmailAddress deskBooked.EmailAddress
            OfficeId = OfficeId deskBooked.OfficeId          
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