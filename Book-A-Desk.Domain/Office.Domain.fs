namespace Book_A_Desk.Domain.Office.Domain

open System

type OfficeId = OfficeId of Guid
type CityName = CityName of string

type Office =
    {
        Id : OfficeId
        City : CityName
        BookableDesksPerDay : int
        OpeningHoursText: string option
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
                BookableDesksPerDay = 34
                OpeningHoursText = "7:30am to 6:30pm from Tuesday to Thursday" |> Some
            }
            {
                Id = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
                City = CityName "Berlin"
                BookableDesksPerDay = 14
                OpeningHoursText = "7:00am to 7:00pm from Tuesday to Thursday" |> Some
            }
        ]
