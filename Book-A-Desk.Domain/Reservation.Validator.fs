namespace Book_A_Desk.Domain.Reservation

open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open System

module BookADeskReservationValidator =    
    let validateEmailIsNotEmpty email =
        match email with
        | EmailAddress "" ->
            Error "The e-mail address must not be empty."
        | _ ->
            Ok ()
            
    let validateDateIsInTheFuture date =
        if date < DateTime.Now then
            Error "Date must be greater than today."
        else
            Ok ()
            
    let validateOfficeIdIsValid officeId =
        if Offices.All |> List.exists (fun office -> office.Id = officeId) |> not then
            Error "You must enter a valid office ID."
        else
            Ok ()
            
    let validateOfficeIsAvailable date officeId reservationAggregate  =
        let isAvailable =
                 match reservationAggregate with
                 | None -> true
                 | Some reservationAggregate ->
                     let maxAllowedBookingsPerOffice =
                         Offices.All
                         |> List.find (fun x -> x.Id = officeId)
                         |> fun x -> x.BookableDesksPerDay

                     reservationAggregate.BookedDesks
                     |> List.filter (fun b -> b.Date = date && b.OfficeId = officeId)
                     |> List.length < maxAllowedBookingsPerOffice
        if isAvailable then
            Ok ()
        else
            Error (sprintf "The office is booked out at %s" (date.ToShortDateString()))
