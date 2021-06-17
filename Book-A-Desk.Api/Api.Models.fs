namespace Book_A_Desk.Api.Models

open System
open Book_A_Desk.Domain.Office.Domain

type UserReference =
    {
        Email: string
    }

type OfficeReference =
    {
        Id: OfficeId
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

type Office =
    {
        Id: string
        Name: string
    }

type OfficeAvailability =
    {
        Id: string
        TotalDesks: int
        AvailableDesks: int
    }

type Offices =
    {
        Items: Office array
    }
