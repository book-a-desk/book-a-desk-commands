namespace Book_A_Desk.Domain.QueriesHandler

open Book_A_Desk.Core
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Office.Queries
open Book_A_Desk.Domain.Reservation
    
module rec OfficeQueriesHandler =        
    let getAll getOffices =
        Ok(getOffices ())
        
    let getAvailabilities getOffices getBookingsForDate (query : GetOfficeAvailabilitiesByDate) = result {
        let offices = getOffices ()
        match List.tryFind (fun (office : Office) -> office.Id = query.OfficeId) offices with
        | None -> return! Error "Could not find office"
        | Some office ->
            let! bookings = getBookingsForDate query.Date
            let bookingsForOffice = List.where (fun booking -> booking.OfficeId = office.Id ) bookings
            let officeAvailability =
                {        
                    Id = office.Id
                    TotalDesks = office.BookableDesksPerDay
                    AvailableDesks = office.BookableDesksPerDay - bookingsForOffice.Length
                }
            return officeAvailability
        }