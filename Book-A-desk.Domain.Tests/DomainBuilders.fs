module rec Book_A_desk.Domain.Tests

    open System
    
    open Book_A_Desk.Domain
    open Book_A_Desk.Domain.Office.Domain
    open Book_A_Desk.Domain.Reservation

    module A =
        let reservationAggregate =
            {
                Id = ReservationAggregate.Id
                BookedDesks = []
            }
    
        let fullyBookedReservationAggregate () =  
            let maxAllowedBookingsPerOffice =
                 Offices.All
                 |> List.sumBy (fun office -> office.BookableDesksPerDay)
            {
                Id = ReservationAggregate.Id
                BookedDesks = [for _ in 1 .. maxAllowedBookingsPerOffice -> booking]
            }
            
        let booking = 
            {
                OfficeId = OfficeId (Guid.NewGuid())
                EmailAddress = EmailAddress "anEmailAddress@fake.com"
                Date = DateTime.MaxValue
            }
            
    module An =
        let office = 
            {
                Id = Guid.NewGuid() |> OfficeId
                City = CityName "Montreal"
                BookableDesksPerDay = 32
            }