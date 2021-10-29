namespace Book_A_Desk.Domain.Reservation

open System

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Errors
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands



module BookADeskReservationValidator =
    
    let private getNumberOfAvailableDesk officeId (offices : Office list) =
        match offices |> List.tryFind (fun x -> x.Id = officeId) with
        | Some office ->
            Ok office.BookableDesksPerDay
        | None ->
            ReservationError.InvalidOfficeId |> Error
    
    let private validateOfficeIsAvailable reservationAggregate officeId getOffices (date : DateTime) = result {   
        let! maxAllowedBookingsPerOffice = getNumberOfAvailableDesk officeId getOffices
        
        let isAvailable =
             reservationAggregate.BookedDesks
             |> List.filter (fun b -> b.Date.Date = date.Date && b.OfficeId = officeId)
             |> List.length < maxAllowedBookingsPerOffice
             
        if isAvailable then
            return ()
        else
            return! date |> ReservationError.OfficeHasNoAvailability |> Error
    }
    
    let private validateUserHasNotBookedYet reservationAggregate emailAddress officeId (date : DateTime) = result {
        let alreadyBookedDesks =
            reservationAggregate.BookedDesks
            |> List.filter(fun bookedDesk -> bookedDesk.Date.Date = date.Date && bookedDesk.OfficeId = officeId && bookedDesk.EmailAddress = emailAddress)
        match alreadyBookedDesks with
        | [] ->
            return ()
        | _ ->
            let userHadBookedBeforeParam : UserBookingParam =
                {
                    Date = date
                    EmailAddress = emailAddress
                }
            return! userHadBookedBeforeParam |> UserHadBookedBefore |> Error
    }
            
    let validateCommand (offices: Office list) (cmd : BookADesk) reservationAggregate domainName = result {
        do! BookADeskValidator.validateCorporateEmail cmd.EmailAddress domainName
        do! BookADeskValidator.validateDateIsGreaterThanToday cmd.Date
        do! BookADeskValidator.validateOfficeIdIsValid cmd.OfficeId offices
        do! validateOfficeIsAvailable reservationAggregate cmd.OfficeId offices cmd.Date
        do! validateUserHasNotBookedYet reservationAggregate cmd.EmailAddress cmd.OfficeId cmd.Date
        
        return ()        
    }
        
