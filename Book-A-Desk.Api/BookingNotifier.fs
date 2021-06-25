namespace Book_A_Desk.Api

open System.Net.Mail
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.QueriesHandler

type BookingNotifier =
    {
        NotifySuccess: Booking -> Async<unit>
    }

module BookingNotifier =
    let provide getEmailServiceConfiguration (smtpClient: SmtpClient) getOffices =         
        let sendEmail mailMessage =
            smtpClient.SendMailAsync(mailMessage)      
                        
        let sendEmailNotification (booking: Booking) =
            async {
                let config = getEmailServiceConfiguration()
                let officeName = OfficeQueriesHandler.getOfficeName booking.Office.Id getOffices
                use mailMessage =
                    new MailMessage(
                        config.EmailSender,
                        booking.User.Email)
                mailMessage.CC.Add config.EmailReviewer
                mailMessage.Subject <- "Book-A-Desk Reservation confirmed"
                mailMessage.Body <- $"You have booked a desk at %s{booking.Date.ToShortDateString()} in the Office %s{officeName}"
                do!
                    sendEmail mailMessage
                    |> Async.AwaitIAsyncResult
                    |> Async.Ignore
                printfn "I've sent a mail message!"
            }
        {            
            NotifySuccess = sendEmailNotification
        }