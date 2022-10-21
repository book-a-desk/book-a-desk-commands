module rec Book_A_desk.Domain.Tests

    open System
    open Book_A_Desk.Domain
    open Book_A_Desk.Domain.Office.Domain
    open Book_A_Desk.Domain.Reservation
    open Book_A_Desk.Domain.Reservation.Domain
    open Book_A_Desk.Domain.Reservation.Events


    module A =
        let booking =
            {
                DeskBooked.OfficeId = OfficeId (Guid.NewGuid())
                EmailAddress = EmailAddress "anEmailAddress@fake.com"
                Date = DateTime.MaxValue
            }

        let reservationAggregate =
            {
                ReservationAggregate.Id = "11111111-1111-1111-1111-111111111111" |> Guid |> ReservationId
                ReservationEvents = [DeskBooked booking]
            }
            
    module An =
        let office = 
            {
                Id = Guid.NewGuid() |> OfficeId
                City = CityName "Montreal"
                BookableDesksPerDay = 32
                OpeningHoursText = "Some Montreal opening hours"
            }
        let anotherOffice =
            {
                Id = Guid.NewGuid() |> OfficeId
                City = CityName "Berlin"
                BookableDesksPerDay = 14
                OpeningHoursText = "Some Berlin opening hours"
            }
