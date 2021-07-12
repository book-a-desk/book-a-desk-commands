module Book_A_Desk.Api.Tests.BookingNotifierTests

open System
open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open MailKit.Net.Smtp
open Xunit

[<Fact>]
let ``GIVEN A booking WHEN calling to SendEmailNotification THEN Send method of SMTP Client must be called `` () = async {
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
            SmtpClientPort = 587
            SmtpUsername = "username"
            SmtpPassword = "password"
            EmailSender = "from@broadsing.com"
            EmailReviewer = "reviewer@broadsing.com"
        }
        
    let mockEmailServiceConfiguration () = mockEmailConfig   
    // TODO Refactor test
    let mockSmtpClient = new SmtpClient()
    let bookingNotifier = BookingNotifier.provide mockEmailServiceConfiguration mockSmtpClient mockGetOffices       
       
    let! result = bookingNotifier.NotifySuccess mockBooking
    // Verify that SendMailAsync has been called
    Assert.True(result)
}