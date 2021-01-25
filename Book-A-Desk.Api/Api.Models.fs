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
