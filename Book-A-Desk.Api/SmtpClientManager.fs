namespace Book_A_Desk.Api

open MailKit.Net.Smtp

type SmtpClientManager =
    {
        SmtpClient: SmtpClient
    }

module SmtpClientManager =
    let provide =         
        let getSmtpClient =
            let smtpClient = new SmtpClient()
            smtpClient.ServerCertificateValidationCallback <- fun _ _ _ _ -> true
            smtpClient
        {
            SmtpClient = getSmtpClient
        }        