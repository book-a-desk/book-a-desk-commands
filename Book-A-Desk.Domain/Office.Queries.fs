namespace Book_A_Desk.Domain.Office.Queries

open System
open Book_A_Desk.Domain.Office.Domain

type GetOfficeAvailabilitiesByDate =
    {        
        OfficeId: OfficeId
        Date: DateTime
    }