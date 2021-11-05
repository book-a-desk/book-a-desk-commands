namespace Book_A_Desk.Domain.Cancellation

open System

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Errors
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands

module BookADeskCancellationValidator =
    
    let private getNumberOfAvailableDesk officeId (offices : Office list) =
        match offices |> List.tryFind (fun x -> x.Id = officeId) with
        | Some office ->
            Ok office.BookableDesksPerDay
        | None ->
            InvalidOfficeId |> Error
    
    let private validateUserAlreadyBooked reservationAggregate emailAddress officeId (date : DateTime) = result {
        let alreadyBookedDesks =
            reservationAggregate.BookedDesks
            |> List.filter(fun bookedDesk -> bookedDesk.Date.Date = date.Date && bookedDesk.OfficeId = officeId && bookedDesk.EmailAddress = emailAddress)
        match alreadyBookedDesks with
        | [] ->
            let userHasNotBookedBeforeParam : UserBookingParam =
                {
                    Date = date
                    EmailAddress = emailAddress
                }
            return! userHasNotBookedBeforeParam |> UserHasNotBookedBefore |> Error
        | _ ->
            return ()
    }
            
    let validateCommand (offices: Office list) (cmd : CancelBookADesk) reservationAggregate domainName = result {
        do! BookADeskValidator.validateCorporateEmail cmd.EmailAddress domainName
        do! BookADeskValidator.validateDateIsGreaterThanToday cmd.Date
        do! BookADeskValidator.validateOfficeIdIsValid cmd.OfficeId offices
        do! validateUserAlreadyBooked reservationAggregate cmd.EmailAddress cmd.OfficeId cmd.Date
        
        return ()        
    }
        
