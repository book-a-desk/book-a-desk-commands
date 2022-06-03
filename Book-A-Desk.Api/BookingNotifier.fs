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

    let newLine = "<br>"

    let healthQuestionnaireMailMessageFr =
            $"Merci de bien vouloir répondre au questionnaire sur l'état de santé. Si vois répondez oui à l'une des questions, nous vous demandons de rester chez vous.{newLine}" +
            $"Si vous avez d’autres questions, n’hésitez pas à écrire à <a href=\"mailto:Anastasia.vlahos@broadsign.com\">Anastasia.vlahos@broadsign.com</a>.{newLine}{newLine}" +
            $"Lien:  <a href=\"https://forms.gle/FyzgXfst8FYRukih9\">Questionnaire sur la santé</a>.{newLine}{newLine}"

    let healthQuestionnaireMailMessageEn =
            $"We ask that you please fill out our health questionnaire. Should you answer yes to any of the questions, we politely ask that you refrain from coming into the office.{newLine}" +
            $"If you have any questions at all, please feel free to reach out to <a href=\"mailto:Anastasia.vlahos@broadsign.com\">Anastasia.vlahos@broadsign.com</a>.{newLine}{newLine}" +
            $"Link:  <a href=\"https://forms.gle/FyzgXfst8FYRukih9\">Health questionnaire</a>.{newLine}{newLine}"

    let vaccinationPolicyMailMessageFr =
        $"Nous souhaitons annoncer 2 changements dans notre politique de vaccination :{newLine}{newLine}" +
        $"1. Tout d’abord, nous allons mettre à jour notre politique pour permettre aux employés et aux visiteurs non vaccinés de venir au bureau sur présentation d’un résultat négatif à un test rapide de dépistage de la COVID-19.{newLine}" +
        $"2. Les employés vaccinés ne devront plus s’inscrire à l’avance pour se rendre au bureau ou remplir le questionnaire de santé. Cependant, vous devrez toujours respecter la politique ci-jointe et vous abstenir de vous rendre au bureau si vous présentez des symptômes de la COVID-19, si vous recevez un test positif à la COVID-19 ou si vous avez été en contact avec quelqu’un qui a reçu un test positif à la COVID-19.{newLine}{newLine}" +
        $"Vous trouverez notre politique de vaccination dans Bamboo.{newLine}{newLine}" +
        $"Si vous avez des questions, n’hésitez pas à communiquer avec moi ou quelqu'un en RH.{newLine}" +
        $"Merci!{newLine}{newLine}"

    let vaccinationPolicyMailMessageEn =
        $"We would like to announce 2 changes to our vaccination policy:{newLine}{newLine}" +
        $"1. First, we will be updating our policy to allow unvaccinated employees and visitors to come to the office upon provision of a negative COVID-19 test result obtained from a rapid test.{newLine}" +
        $"2. Vaccinated employees will no longer have to register in advance to go to the office or fill out the health questionnaire. However, they will still be expected to follow the attached policy and refrain from going into the office if they are experiencing symptoms that would lead them to believe that they could have COVID-19, tested positive for COVID-19 or were exposed to someone who tested positive.{newLine}{newLine}" +
        $"Please find our updated vaccination policy in Bamboo.{newLine}{newLine}" +
        $"For any questions, don't be shy to reach out to myself or any of us in HR.{newLine}" +
        $"Thank you!{newLine}{newLine}"

    let createMailText (bookingDate : DateTime) (office : Office) =
            let (CityName officeName) = office.City
            let bookingDateFr = bookingDate.ToString("dd/MM/yyyy")
            let bookingDateEn = bookingDate.ToString("MM/dd/yyyy")

            $"Vous avez réservé un bureau le %s{bookingDateFr} dans le bureau %s{officeName} Heures d'ouverture : {office.OpeningHoursText}.{newLine}" +
            $"Vous êtes responsable de vérifier que le bureau est ouvert à la date que vous avez réservée.{newLine}{newLine}" +
            vaccinationPolicyMailMessageFr +
            $"------------------------------------------{newLine}" +
            $"You have booked a desk at %s{bookingDateEn} in the Office %s{officeName} Opening hours: {office.OpeningHoursText}.{newLine}" +
            $"It is your responsibility to verify that the office is open for the date that you have booked.{newLine}{newLine}" +
            vaccinationPolicyMailMessageEn

    let createOfficeRestriction (office : Office) =
            let (CityName officeName) = office.City

            $"Nous vous informons que le bureau %s{officeName} a certaines restrictions.{newLine}" +
            $"Veuillez lire attentivement les dernières mises à jour de la politique de vaccination :{newLine}{newLine}" +
            vaccinationPolicyMailMessageFr +
            $"------------------------------------------{newLine}" +
            $"We inform you that the Office %s{officeName} has some restrictions.{newLine}" +
            $"Please read carefully the last updates on vaccination policy:{newLine}{newLine}" +
            vaccinationPolicyMailMessageEn
            
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