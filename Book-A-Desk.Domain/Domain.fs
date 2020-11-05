namespace Bookadesk.Commands.Domain
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

type BookADeskCommand =
    | BookADesk of BookADesk
//    | UnbookAdesk Of UnbookAdesk

//Events
type DeskBooked =
    {
        Date: DateTime
        EmailAddress: EmailAddress
        OfficeID: OfficeID
        AggregateId : Guid //
    }

type BookADeskEvent =
    | DeskBooked of DeskBooked


type BookADeskCommandHandler =
    {
        Handle: BookADeskCommand -> Result<BookADeskEvent,string>
    }




module BookADeskCommandHandler =
   let provide storeEvent =
       let handle (command : BookADeskCommand) =


            match command with
            | BookADesk command ->
                if command.EmailAddress = EmailAddress "" then
                    Error "The e-mail address must not be empty."
                else if command.Date > DateTime.Now then
                    Error "Date must be greater than today."
                else if command.OfficeID <> OfficeID "Berlin" && command.OfficeID <> OfficeID "Montreal" then
                    Error "You must enter a valid office ID."
                else
                    //Create the event.
                    let aggregateId = Guid.NewGuid()
                    let event =
                        {
                            DeskBooked.AggregateId = aggregateId
                            Date = command.Date
                            EmailAddress = command.EmailAddress
                            OfficeID = command.OfficeID
                        }
                        |> DeskBooked

                    match storeEvent (aggregateId, event) with
                    | Ok _ -> Ok event
                    | Error e -> Error e
            | _ -> failwith " ........"


       {
            Handle = handle
       }


module InMemoryEventStore =
    open System.Collections.Generic

    let store = Dictionary<Guid, BookADeskEvent> ()

    let storeEvent (aggregateId, event) =
        try
            store.Add (aggregateId, event)
            Ok ()
        with
        | _ -> Error "Error while storing event"
