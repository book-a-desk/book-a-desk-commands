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
            
    let private validateDateIsInTheFuture date =
        if date < DateTime.Now then
            Error "Date must be greater than today."
        else
            Ok ()
            
    let private validateOfficeIdIsValid officeId getOffices =
        if getOffices () |> List.exists (fun office -> office.Id = officeId) |> not then
            Error "You must enter a valid office ID."
        else
            Ok ()
             
    let private validateReservationAggregate reservationAggregate =
        match reservationAggregate with
        | Some reservationAggregate -> Ok reservationAggregate
        | None -> Error "Could not get reservation aggregate"
    
    let private getNumberOfAvailableDesk officeId getOffices =
        match getOffices () |> List.tryFind (fun x -> x.Id = officeId) with
         | Some office ->
             Ok office.BookableDesksPerDay
         | None ->
             Error "You must enter a valid office ID."
    
    let private validateOfficeIsAvailable reservationAggregate officeId getOffices (date : DateTime) = result {
        let! reservationAggregate = validateReservationAggregate reservationAggregate                
        let! maxAllowedBookingsPerOffice = getNumberOfAvailableDesk officeId getOffices
        
        let isAvailable =
             reservationAggregate.BookedDesks
             |> List.filter (fun b -> b.Date.Date = date.Date && b.OfficeId = officeId)
             |> List.length < maxAllowedBookingsPerOffice
             
        if isAvailable then
            return ()
        else
            return! Error (sprintf "The office is booked out at %s" (date.ToShortDateString()))
    }
            
    let validateCommand getOffices (cmd : BookADesk) reservationAggregate = result {
        do! validateEmailIsNotEmpty cmd.EmailAddress
        do! validateDateIsInTheFuture cmd.Date
        do! validateOfficeIdIsValid cmd.OfficeId getOffices
        do! validateOfficeIsAvailable reservationAggregate cmd.OfficeId getOffices cmd.Date
        
        return ()        
    }
        
