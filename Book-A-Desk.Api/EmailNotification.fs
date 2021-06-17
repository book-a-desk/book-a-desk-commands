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
    let initialize getEmailServiceConfiguration getOffices =         
        let sendEmailNotification (booking: Booking) =
            let config = getEmailServiceConfiguration()        
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
                    
            use smtpClient = new SmtpClient(config.SmtpClientUrl)
            smtpClient.EnableSsl <- true
            smtpClient.Credentials <- System.Net.NetworkCredential(config.SmtpUsername, config.SmtpPassword)            
            use mailMessage =
                new MailMessage(
                    config.EmailSender,
                    booking.User.Email)
            mailMessage.CC.Add config.EmailReviewer
            mailMessage.Subject <- "Book-A-Desk Reservation confirmed"
            mailMessage.Body <- $"You have booked a desk at %s{booking.Date.ToShortDateString()} in the Office %s{officeName}"

            smtpClient.Send(mailMessage)
            printfn "I've sent a mail message!"  
        {            
            SendEmailNotification = sendEmailNotification
        }        