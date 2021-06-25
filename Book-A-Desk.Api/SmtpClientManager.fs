namespace Book_A_Desk.Api

open System.Net.Mail

type SmtpClientManager =
    {
        SmtpClient: SmtpClient
    }

module SmtpClientManager =
    let provide getEmailServiceConfiguration =         
        let getSmtpClient =
            let config = getEmailServiceConfiguration()
            use smtpClient = new SmtpClient(config.SmtpClientUrl)
            smtpClient.EnableSsl <- true
            smtpClient.Credentials <- System.Net.NetworkCredential(config.SmtpUsername, config.SmtpPassword)
            smtpClient
        {
            SmtpClient = getSmtpClient
        }        