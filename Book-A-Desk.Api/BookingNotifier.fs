namespace Book_A_Desk.Api

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
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
            async {
                smtpClient.Connect(config.SmtpClientUrl, config.SmtpClientPort, SecureSocketOptions.StartTlsWhenAvailable)
                smtpClient.Authenticate(config.SmtpUsername, config.SmtpPassword)                            
                do!
                    smtpClient.SendAsync mailMessage
                        |> Async.AwaitIAsyncResult
                        |> Async.Ignore
                smtpClient.Disconnect(true)
            }          
                                
        let sendEmailNotification (booking: Booking) =            
                let config = getEmailServiceConfiguration()
                let (CityName officeName) = OfficeQueriesHandler.getOfficeName booking.Office.Id getOffices
                
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
                
                sendEmail config mailMessage
        {            
            NotifySuccess = sendEmailNotification
        }