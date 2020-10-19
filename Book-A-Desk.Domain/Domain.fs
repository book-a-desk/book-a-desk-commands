namespace bookadesk.commands.domain
open System

type OfficeID = OfficeID of string
type EmailAddress = EmailAddress of string

type BookADesk =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeID: OfficeID
    }

module BookADeskCommandHandler = 
    let Handle command = 
    //
    //Implementation will follow here.
    //
        if command.EmailAddress = EmailAddress "" then
            Error "The e-mail address must not be empty."
        else if command.Date > DateTime.now then
            Error "Date must be greater than today."
        else if command.OfficeID != OfficeID "Berlin" && commands.OfficeID != OfficeID "Montreal" then
            Error "You must enter a valid ofiice ID."
        else
            //Create the event.


            Ok
        

