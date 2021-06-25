module Book_A_Desk.Api.Tests.EmailNotificationTests

open System
open System.Net.Mail
open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Xunit

[<Fact>]
let ``GIVEN A booking WHEN calling to SendEmailNotification THEN Email must be sent`` () = async {
    let officeId = Guid.NewGuid ()
    let totalDesks = 32
    let mockOffice =
        {
            Id = officeId |> OfficeId
            City = CityName "SomeCityName"
            BookableDesksPerDay = totalDesks
        }
    let mockedOfficeReference =
        {
            Id = officeId.ToString()
        }
    let offices =
        mockOffice |> List.singleton
    let mockGetOffices () = offices
    
    let date = DateTime(2021,02,01)
    
    let mockedUser =
        {
            Email = "booking.user@broadsing.com"    
        }
    
    let mockBooking =
        {
            Office = mockedOfficeReference
            Date = date
            User = mockedUser
        }
        
    let mockEmailConfig =
        {
            SmtpClientUrl = "http://localhost:8080/SMTP"
            SmtpUsername = "username"
            SmtpPassword = "password"
            EmailSender = "from@broadsing.com"
            EmailReviewer = "reviewer@broadsing.com"
        }
        
    let mockEmailServiceConfiguration () = mockEmailConfig
    let mockSmtpClient = new SmtpClient(mockEmailConfig.SmtpClientUrl)        
    let emailNotification = EmailNotification.initialize mockEmailServiceConfiguration mockSmtpClient mockGetOffices       
    
    // Refactor this code to mock SmtpClient and verify that Send function is called
    try
        emailNotification.SendEmailNotification mockBooking
        Assert.False(true)
    with
    | :? SmtpException as ex ->
        match ex.TargetSite.Name with
        | "Send" -> printfn "I've sent a mail message! We know that host is not valid."
        | _ -> Assert.False(true)
}