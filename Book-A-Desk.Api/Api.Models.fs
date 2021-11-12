namespace Book_A_Desk.Api.Models

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain

type UserReference =
    {
        Email: string
    }

type OfficeReference =
    {
        Id: string
    }

type InputBooking =
    {
        Office: OfficeReference
        Date: DateTime
        User: UserReference
    }

type Booking =
    {
        Office: OfficeReference
        Date: DateTime
        User: UserReference
    }
module Booking =
    
    let private officeIdValue (OfficeId e) = e
    
    let private emailAddressValue (EmailAddress e) = e
    
    let value (officeId:OfficeId) (date: DateTime) (email:EmailAddress) =
        {
            Office = { Id = officeId |> officeIdValue |> string }
            Date = date
            User = { Email = email |> emailAddressValue }
        }
        
    
    
type OpeningHours =
    {
        Text: string
    }

type Office =
    {
        Id: string
        Name: string
        OpeningHours : OpeningHours
    }

type OfficeAvailability =
    {
        Id: string
        TotalDesks: int
        ReservedDesks: int
        AvailableDesks: int
    }

type Offices =
    {
        Items: Office array
    }
