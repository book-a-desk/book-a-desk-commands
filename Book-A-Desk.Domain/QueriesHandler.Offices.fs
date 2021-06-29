namespace Book_A_Desk.Domain.QueriesHandler

open System
open Book_A_Desk.Core
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Office.Queries
open Book_A_Desk.Domain.Reservation
    
module rec OfficeQueriesHandler =        
    let getAll getOffices =
        Ok(getOffices ())
        
    let getAvailabilities getOffices getBookingsForDate minimumDate (query : GetOfficeAvailabilitiesByDate) = result {
        let offices = getOffices ()
        match List.tryFind (fun (office : Office) -> office.Id = query.OfficeId) offices with
        | None -> return! Error "Could not find office"
        | Some office ->
            match query.Date with
            // If the date is in the past or the same as today, there are no availabilities
            | date when date.Date <= minimumDate ->
                let! bookings = getBookingsForDate query.Date
                let bookingsForOffice = List.where (fun booking -> booking.OfficeId = office.Id ) bookings
                let officeAvailability =
                    {        
                        Id = office.Id
                        TotalDesks = office.BookableDesksPerDay
                        ReservedDesks = bookingsForOffice.Length
                        AvailableDesks = 0
                    }
                return officeAvailability
                
            | _ ->
                let! bookings = getBookingsForDate query.Date
                let bookingsForOffice = List.where (fun booking -> booking.OfficeId = office.Id ) bookings
                let officeAvailability =
                    {        
                        Id = office.Id
                        TotalDesks = office.BookableDesksPerDay
                        ReservedDesks = bookingsForOffice.Length
                        AvailableDesks = office.BookableDesksPerDay - bookingsForOffice.Length
                    }
                return officeAvailability
        }
    
    let getOfficeName (officeReference: string) getOffices =
        let result = getAll getOffices
        let officeReference = Guid.Parse(officeReference) |> OfficeId
        match result with
        | Ok offices ->
            offices
            |> List.tryFind (fun (o:Office) -> officeReference.Equals(o.Id))
            |> function
                | None -> "Unknown"
                | Some (o:Office) -> o.City.ToString()
        | Error e ->
            "Unknown"