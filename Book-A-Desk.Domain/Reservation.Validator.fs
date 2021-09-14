namespace Book_A_Desk.Domain.Reservation

open System

open System.ComponentModel.DataAnnotations
open System.Text.RegularExpressions
open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands

type UserHadBookedBeforeParam =
    {
        Date: DateTime
        EmailAddress: EmailAddress        
    }

type ReservationError =
    | InvalidEmailAddress
    | DateLowerThanToday
    | InvalidOfficeId
    | OfficeHasNoAvailability of DateTime
    | UserHadBookedBefore of UserHadBookedBeforeParam

module BookADeskReservationValidator =
    
    let private validateCorporateEmail email validDomainName =
        let (EmailAddress emailToValidate) = email
        let emailValidator = EmailAddressAttribute()
        let isValidEmail = emailValidator.IsValid(emailToValidate)
        let domainName = "@" + validDomainName
        let hasCorporateDomain = Regex.Match(emailToValidate.ToLower(), domainName)
        if isValidEmail && hasCorporateDomain.Success then
            Ok()
        else
            InvalidEmailAddress |> Error
            
    let private validateDateIsGreaterThanToday requestedDate =
        let allowedDate = DateTime.Today.AddDays(1.)
        if requestedDate < allowedDate then
            DateLowerThanToday |> Error
        else
            Ok ()
            
    let private validateOfficeIdIsValid officeId (offices : Office list) =
        if offices |> List.exists (fun office -> office.Id = officeId) |> not then
            InvalidOfficeId |> Error
        else
            Ok ()
    
    let private getNumberOfAvailableDesk officeId (offices : Office list) =
        match offices |> List.tryFind (fun x -> x.Id = officeId) with
        | Some office ->
            Ok office.BookableDesksPerDay
        | None ->
            InvalidOfficeId |> Error
    
    let private validateOfficeIsAvailable reservationAggregate officeId getOffices (date : DateTime) = result {   
        let! maxAllowedBookingsPerOffice = getNumberOfAvailableDesk officeId getOffices
        
        let isAvailable =
             reservationAggregate.BookedDesks
             |> List.filter (fun b -> b.Date.Date = date.Date && b.OfficeId = officeId)
             |> List.length < maxAllowedBookingsPerOffice
             
        if isAvailable then
            return ()
        else
            return! date |> OfficeHasNoAvailability |> Error
    }
    
    let private validateUserHasNotBookedYet reservationAggregate emailAddress officeId (date : DateTime) = result {
        let alreadyBookedDesks =
            reservationAggregate.BookedDesks
            |> List.filter(fun bookedDesk -> bookedDesk.Date.Date = date.Date && bookedDesk.OfficeId = officeId && bookedDesk.EmailAddress = emailAddress)
        match alreadyBookedDesks with
        | [] ->
            return ()
        | _ ->
            let userHadBookedBeforeParam =
                {
                    Date = date
                    EmailAddress = emailAddress
                }
            return! userHadBookedBeforeParam |> UserHadBookedBefore |> Error
    }
            
    let validateCommand (offices: Office list) (cmd : BookADesk) reservationAggregate domainName = result {
        do! validateCorporateEmail cmd.EmailAddress domainName
        do! validateDateIsGreaterThanToday cmd.Date
        do! validateOfficeIdIsValid cmd.OfficeId offices
        do! validateOfficeIsAvailable reservationAggregate cmd.OfficeId offices cmd.Date
        do! validateUserHasNotBookedYet reservationAggregate cmd.EmailAddress cmd.OfficeId cmd.Date
        
        return ()        
    }
        
