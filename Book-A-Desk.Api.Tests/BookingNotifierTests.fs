module Book_A_Desk.Api.Tests.BookingNotifierTests

open System
open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open FSharp.Control.Tasks
open Foq
open FsToolkit.ErrorHandling
open MailKit.Net.Smtp
open Xunit

[<Fact>]
let ``GIVEN A booking WHEN calling to SendEmailNotification THEN SendAsync method of SMTP Client must be called `` () = async {
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
    
    // Mock SmtpClient using Foq
    let mutable sendWasCalled = false
    let mockSmtpClient = Mock<SmtpClient>()
                             .Setup(fun x -> <@ x.Connect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Disconnect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Authenticate(mockEmailConfig.SmtpUsername, mockEmailConfig.SmtpPassword) @>).Returns(())
                             .Setup(fun x -> <@ x.SendAsync(any()) @>).Returns(task {sendWasCalled <- true})
                             .Create()
                             
    let bookingNotifier = BookingNotifier.provide mockEmailServiceConfiguration mockSmtpClient mockGetOffices       
       
    let! result = bookingNotifier.NotifySuccess mockBooking
    Assert.True(result)
    Assert.True(sendWasCalled)
}