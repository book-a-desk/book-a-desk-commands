namespace Book_A_Desk.Api

type EmailServiceConfiguration =
    {
            SmtpClientUrl: string
            SmtpClientPort: int
            SmtpUsername: string
            SmtpPassword: string
            EmailSender: string
            EmailReviewer: string
    }
