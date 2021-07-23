namespace Book_A_Desk.Domain.Reservation

open System

open Book_A_Desk.Core
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation.Commands

module BookADeskReservationValidator =
    
    let private validateEmailIsNotEmpty email =
        match email with
        | EmailAddress "" ->
            Error "The e-mail address must not be empty."
        | _ ->
            Ok ()
            
    let private validateDateIsGreaterThanToday requestedDate =
        let allowedDate = DateTime.Today.AddDays(1.)
        if requestedDate < allowedDate then
            Error "Date must be greater than today."
        else
            Ok ()
            
    let private validateOfficeIdIsValid officeId (offices : Office list) =
        if offices |> List.exists (fun office -> office.Id = officeId) |> not then
            Error "You must enter a valid office ID."
        else
            Ok ()
    
    let private getNumberOfAvailableDesk officeId (offices : Office list) =
        match offices |> List.tryFind (fun x -> x.Id = officeId) with
         | Some office ->
             Ok office.BookableDesksPerDay
         | None ->
             Error "You must enter a valid office ID."
    
    let private validateOfficeIsAvailable reservationAggregate officeId getOffices (date : DateTime) = result {   
        let! maxAllowedBookingsPerOffice = getNumberOfAvailableDesk officeId getOffices
        
        let isAvailable =
             reservationAggregate.BookedDesks
             |> List.filter (fun b -> b.Date.Date = date.Date && b.OfficeId = officeId)
             |> List.length < maxAllowedBookingsPerOffice
             
        if isAvailable then
            return ()
        else
            return! Error ($"The office is booked out at {date.ToShortDateString()}" )
    }
    
    let private validateUserHasNotBookedYet reservationAggregate emailAddress officeId (date : DateTime) = result {
        let alreadyBooked =
            reservationAggregate.BookedDesks
            |> List.filter(fun b -> b.Date.Date = date.Date && b.OfficeId = officeId && b.EmailAddress = emailAddress)
        match alreadyBooked with
        | [] ->
            return ()
        | _ ->
            return! Error ($"The office is already booked out at {date.ToShortDateString()} for user {emailAddress}")
    }
            
    let validateCommand (offices: Office list) (cmd : BookADesk) reservationAggregate = result {
        do! validateEmailIsNotEmpty cmd.EmailAddress
        do! validateDateIsGreaterThanToday cmd.Date
        do! validateOfficeIdIsValid cmd.OfficeId offices
        do! validateOfficeIsAvailable reservationAggregate cmd.OfficeId offices cmd.Date
        do! validateUserHasNotBookedYet reservationAggregate cmd.EmailAddress cmd.OfficeId cmd.Date
        
        return ()        
    }
        
