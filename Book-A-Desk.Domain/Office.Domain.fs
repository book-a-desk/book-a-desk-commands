namespace Book_A_Desk.Domain.Office.Domain

open System

type OfficeId = OfficeId of Guid
type CityName = CityName of string

type Office =
    {
        Id : OfficeId
        City : CityName
        BookableDesksPerDay : int
    }
    
type OfficeAvailability =
    {        
        Id : OfficeId
        TotalDesks : int
        ReservedDesks: int
        AvailableDesks : int
    }
    
module Offices =
    let All =
        [
            {
                Id = Guid.Parse("4B774D13-645B-4378-A925-1DA565A35FD7") |> OfficeId
                City = CityName "Montreal"
                BookableDesksPerDay = 32
            }
            {
                Id = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
                City = CityName "Berlin"
                BookableDesksPerDay = 14
            }
        ]
