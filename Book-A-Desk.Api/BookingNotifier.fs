namespace Book_A_Desk.Api

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.QueriesHandler
open MailKit.Net.Smtp
open MailKit.Security
open MimeKit

type BookingNotifier =
    {
        NotifySuccess: Booking -> Async<unit>
    }

module BookingNotifier =
    let provide getEmailServiceConfiguration (smtpClient: SmtpClient) getOffices =         
        let sendEmail config mailMessage =
            smtpClient.Connect(config.SmtpClientUrl, 587, SecureSocketOptions.StartTlsWhenAvailable)
            smtpClient.Authenticate(config.SmtpUsername, config.SmtpPassword)                            
            smtpClient.Send mailMessage
            smtpClient.Disconnect(true);
                        
        let sendEmailNotification (booking: Booking) =
            async {            
                let config = getEmailServiceConfiguration()
                let officeName = OfficeQueriesHandler.getOfficeName booking.Office.Id getOffices
                
                let mailMessage = MimeMessage()            
                MailboxAddress.Parse(config.EmailSender)
                |> mailMessage.From.Add            
                MailboxAddress.Parse(booking.User.Email)
                |> mailMessage.To.Add
                MailboxAddress.Parse(config.EmailReviewer)
                |> mailMessage.Cc.Add
                
                mailMessage.Subject <- "Book-A-Desk Reservation confirmed"
                let bodyPart = TextPart("plain")
                bodyPart.Text <- $"You have booked a desk at %s{booking.Date.ToShortDateString()} in the Office %s{officeName}"
                mailMessage.Body <- bodyPart
                
                do!
                    sendEmail config mailMessage
                            |> Async.AwaitIAsyncResult
                            |> Async.Ignore
            }                                
        {            
            NotifySuccess = sendEmailNotification
        }