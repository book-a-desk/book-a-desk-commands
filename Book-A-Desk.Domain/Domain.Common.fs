namespace Book_A_Desk.Domain

open System

type EmailAddress = EmailAddress of string
type EmailSubject = EmailSubject of string
type EmailBody = EmailBody of string

type EmailMessage =
    {
        ToAddress : EmailAddress
        Subject : EmailSubject
        Body : EmailBody
    }
