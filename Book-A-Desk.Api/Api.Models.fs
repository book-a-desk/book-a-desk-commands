﻿namespace Book_A_Desk.Api.Models

open System

type Booking =
    {
        OfficeId: string
        BookingDate: DateTime
        EmailAddress: string
    }

