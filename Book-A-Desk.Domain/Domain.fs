namespace bookadesk.commands.domain
open System

type OfficeID = OfficeID of string
type EmailAddress = EmailAddress of string

//Commands
type BookADesk =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeID: OfficeID
    }

//Events
type DeskBooked =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeID: OfficeID
    }
    
module BookADeskCommandHandler =
    let Handle command storeEvent =
        if command.EmailAddress = EmailAddress "" then
            Error "The e-mail address must not be empty."
        else if command.Date > DateTime.Now then
            Error "Date must be greater than today."
        else if command.OfficeID <> OfficeID "Berlin" && command.OfficeID <> OfficeID "Montreal" then
            Error "You must enter a valid office ID."
        else
            //Create the event.
            let event =
                {
                    DeskBooked.Date = command.Date
                    EmailAddress = command.EmailAddress
                    OfficeID = command.OfficeID
                }
           
            match storeEvent event with
            | Ok _ -> Ok event
            | Error e -> Error e

