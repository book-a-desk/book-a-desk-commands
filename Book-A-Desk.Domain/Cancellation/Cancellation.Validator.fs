namespace Book_A_Desk.Domain.Cancellation

open System

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Errors
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Events

module BookADeskCancellationValidator =
    
    let private getNumberOfAvailableDesk officeId (offices : Office list) =
        match offices |> List.tryFind (fun x -> x.Id = officeId) with
        | Some office ->
            Ok office.BookableDesksPerDay
        | None ->
            InvalidOfficeId |> Error
    
    let private validateUserAlreadyBooked reservationAggregate emailAddress officeId (date : DateTime) = result {
        let alreadyBookedDesks =
            reservationAggregate.ReservationEvents
            |> List.filter (fun reservationEvent ->
                 // TODO: For every reservation aggregate check if last status is booked and check if it is DeskBooked
                 match reservationEvent with
                 | DeskBooked bookedDesk ->
                     bookedDesk.Date.Date = date.Date && bookedDesk.OfficeId = officeId && bookedDesk.EmailAddress = emailAddress
                 | _ -> false)

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
        
