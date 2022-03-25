namespace Book_A_Desk.Api

open FsToolkit.ErrorHandling
open MailKit.Net.Smtp
open MailKit.Security
open MimeKit
open System
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.QueriesHandler
open MimeKit.Text

type BookingNotifier =
    {
        NotifySuccess: Booking -> Async<Result<unit, string>>
        NotifyOfficeRestrictionToBooking: Booking -> Async<Result<unit, string>>
    }
    
type EmailNotificationDetails =
    {
        From: string
        To: string
        EmailReviewer: string
        Subject: string
        Text: string
    }

module rec BookingNotifier =
    let provide getEmailServiceConfiguration (smtpClient: SmtpClient) getOffices =         
        let sendEmail config mailMessage =
            asyncResult {
                smtpClient.Connect(config.SmtpClientUrl, config.SmtpClientPort, SecureSocketOptions.StartTlsWhenAvailable)
                smtpClient.Authenticate(config.SmtpUsername, config.SmtpPassword)
                let! response = smtpClient.SendAsync mailMessage |> Async.AwaitIAsyncResult
                smtpClient.Disconnect(true)
                match response with
                | true -> return ()
                | false -> return! Error "Could not send email message"
            }
        
        let sendOfficeRestrictionNotification (booking: Booking) = asyncResult {
                let config = getEmailServiceConfiguration()
                let! office = getOffice booking.Office.Id getOffices
                let mailText = createOfficeRestriction office
                let (CityName officeName) = office.City
                   
                let emailNotificationDetails =
                    {
                        From = config.EmailSender
                        To = booking.User.Email
                        EmailReviewer = String.Empty
                        Subject = $"Book-A-Desk Office %s{officeName} Restrictions"
                        Text = mailText                          
                    }
                
                let mailMessage = createEmailMessage emailNotificationDetails
                
                return! sendEmail config mailMessage
            }
        
        let sendEmailNotification (booking: Booking) = asyncResult {
                let config = getEmailServiceConfiguration()
                let! office = getOffice booking.Office.Id getOffices
                let mailText = createMailText booking.Date office
                let emailNotificationDetails =
                    {
                        From = config.EmailSender
                        To = booking.User.Email
                        EmailReviewer = config.EmailReviewer
                        Subject = "Book-A-Desk Reservation confirmed"
                        Text = mailText
                    }
                
                let mailMessage = createEmailMessage emailNotificationDetails
                
                return! sendEmail config mailMessage
            }
        {            
            NotifySuccess = sendEmailNotification
            NotifyOfficeRestrictionToBooking = sendOfficeRestrictionNotification
        }
        
    let createEmailMessage emailNotificationDetails =
        let mailMessage = MimeMessage()            
        MailboxAddress.Parse(emailNotificationDetails.From)
        |> mailMessage.From.Add            
        MailboxAddress.Parse(emailNotificationDetails.To)
        |> mailMessage.To.Add
        if emailNotificationDetails.EmailReviewer <> String.Empty then
            MailboxAddress.Parse(emailNotificationDetails.EmailReviewer)
            |> mailMessage.Cc.Add
        
        mailMessage.Subject <- emailNotificationDetails.Subject
        let bodyPart = TextPart(TextFormat.Html)
        
        bodyPart.Text <- emailNotificationDetails.Text
        mailMessage.Body <- bodyPart
        mailMessage

    let healthQuestionnaireMailMessage =
            $"SVP, veuillez répondre au questionnaire sur l’état de santé. Si vous répondez oui à une des questions, nous vous demandons de rester à la maison.{Environment.NewLine}{Environment.NewLine}" +
            $"Si vous avez d’autres questions, n’hésitez pas à écrire à <a href=\"mailto:Anastasia.vlahos@broadsign.com\">Anastasia.vlahos@broadsign.com</a>.{Environment.NewLine}{Environment.NewLine}" +
            $"We ask that you please fill out our health questionnaire. Should you answer yes to any of the questions, we politely ask that you refrain from coming into the office.{Environment.NewLine}{Environment.NewLine}" +
            $"If you have any questions at all, please feel free to reach out to <a href=\"mailto:Anastasia.vlahos@broadsign.com\">Anastasia.vlahos@broadsign.com</a>.{Environment.NewLine}{Environment.NewLine}" +
            $"Link:  <a href=\"https://forms.gle/FyzgXfst8FYRukih9\">Questionnaire sur la Santé // Health Questionnaire</a>.{Environment.NewLine}"

    let createMailText (bookingDate : DateTime) (office : Office) =
            let (CityName officeName) = office.City
            $"You have booked a desk at %s{bookingDate.ToShortDateString()} in the Office %s{officeName}{Environment.NewLine}" +
            $"Opening hours: {office.OpeningHoursText}{Environment.NewLine}" +
            $"It is your responsibility to verify that the office is open for the date that you have booked.{Environment.NewLine}{Environment.NewLine}" +
            healthQuestionnaireMailMessage

    let createOfficeRestriction (office : Office) =
            let (CityName officeName) = office.City
            $"We inform you that the Office %s{officeName} has some restrictions.{Environment.NewLine}" +
            $"If you have not already filled the health questionnaire, please follow the next instructions:{Environment.NewLine}{Environment.NewLine}" +
            healthQuestionnaireMailMessage
            
    let getOffice officeId getOffices : Result<_,_> = result {
        let! officeId =
            InputParser.parseOfficeId officeId
            |> Result.mapError (fun _ -> "OfficeId is invalid")
        let! office = OfficeQueriesHandler.getOfficeById officeId getOffices
        
        match office with
        | Some office ->
            return office
        | None ->
            return! Error "Could not find Office"        
    }