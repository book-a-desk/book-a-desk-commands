module rec Book_A_desk.Domain.Tests

    open System
    
    open Book_A_Desk.Domain
    open Book_A_Desk.Domain.Office.Domain
    open Book_A_Desk.Domain.Reservation

    module ReservationAggregate  =
        let Create =            
            {
                Id = ReservationAggregate.Id
                BookedDesks = []
            }
            |> Some
            
        let CreateFullyBooked () =  
            let maxAllowedBookingsPerOffice =
                 Offices.All
                 |> List.sumBy (fun office -> office.BookableDesksPerDay)
            {
                Id = ReservationAggregate.Id
                BookedDesks = [for _ in 1 .. maxAllowedBookingsPerOffice -> Booking.Create]
            }
            |> Some
            
    module Booking =
        let Create =
            {
                OfficeId = OfficeId (Guid.NewGuid())
                EmailAddress = EmailAddress "anEmailAddress@fake.com"
                Date = DateTime.MaxValue
            }
            
    module Office =
        let Create =
            {
                Id = Guid.NewGuid() |> OfficeId
                City = CityName "Montreal"
                BookableDesksPerDay = 32
            }