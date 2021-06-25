namespace Book_A_Desk.Api

open System.Net.Mail
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.QueriesHandler

type EmailNotification =
    {
        SendEmailNotification: Booking -> unit
    }

module EmailNotification =
    let initialize getEmailServiceConfiguration (smtpClient: SmtpClient) getOffices =         
        let sendEmail mailMessage =
            smtpClient.Send(mailMessage)           
                        
        let sendEmailNotification (booking: Booking) =
            let config = getEmailServiceConfiguration()
            let officeName = OfficeQueriesHandler.getOfficeName booking.Office.Id getOffices
            use mailMessage =
                new MailMessage(
                    config.EmailSender,
                    booking.User.Email)
            mailMessage.CC.Add config.EmailReviewer
            mailMessage.Subject <- "Book-A-Desk Reservation confirmed"
            mailMessage.Body <- $"You have booked a desk at %s{booking.Date.ToShortDateString()} in the Office %s{officeName}"            
            sendEmail mailMessage
            printfn "I've sent a mail message!"  
        {            
            SendEmailNotification = sendEmailNotification
        }