﻿namespace Book_A_Desk.Api.Models

open System

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

type Office =
    {
        Id: string
        Name: string
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
