namespace Book_A_Desk.Api

open System.Net.Mail
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.QueriesHandler
open Book_A_Desk.Domain.Office.Domain

type EmailNotification =
    {
        SendEmailNotification: Booking -> unit
    }

module EmailNotification =
    let initialize getOffices =         
        let sendEmailNotification (booking: Booking) = 
            let officeName =
                let result = OfficeQueriesHandler.getAll getOffices
                match result with
                | Ok offices ->
                    offices
                    |> List.find (fun (o:Office) -> booking.Office.Id.Equals(o.Id))
                    |> (fun (o:Office) -> 
                        let (CityName cityName) = o.City
                        cityName)
                | Error e ->
                    "Unknown"
            // To be refactored to extract harcoded variables
            use smtpClient = new SmtpClient("smtp.local")
            use mailMessage =
                new MailMessage(
                    "reception@broadsign.com",
                    booking.User.Email)
            mailMessage.CC.Add "karol.yuen@broadsign.com"
            mailMessage.Subject <- "Book-A-Desk Reservation confirmed"
            mailMessage.Body <- $"You have booked a desk at %s{booking.Date.ToShortDateString()} in the Office %s{officeName}"

            smtpClient.Send(mailMessage)
            printfn "I've sent a mail message!"  
        {            
            SendEmailNotification = sendEmailNotification
        }        