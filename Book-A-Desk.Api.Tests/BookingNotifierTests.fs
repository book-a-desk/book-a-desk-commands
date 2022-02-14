module Book_A_Desk.Api.Tests.BookingNotifierTests

open System
open System.Threading.Tasks
open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Foq
open FsToolkit.ErrorHandling
open MailKit.Net.Smtp
open Xunit

let officeId = Guid.NewGuid ()

let totalDesks = 32
let mockOffice =
    {
        Id = officeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = totalDesks
        OpeningHoursText = "some opening hours"
    }
let mockedOfficeReference =
    {
        Id = officeId.ToString()
    }
let offices = mockOffice |> List.singleton
let mockGetOffices () = offices

let date = DateTime(2021,02,01)

let mockedUser =
    {
        Email = "booking.user@domain.com"
    }

let mockBooking =
    {
        Booking.Office = mockedOfficeReference
        Date = date
        User = mockedUser
    }

let mockEmailConfig =
    {
        SmtpClientUrl = "http://localhost:8080/SMTP"
        SmtpClientPort = 587
        SmtpUsername = "username"
        SmtpPassword = "password"
        EmailSender = "from@domain.com"
        EmailReviewer = "reviewer@domain.com"
    }

let mockEmailServiceConfiguration () = mockEmailConfig

[<Fact>]
let ``GIVEN A booking WHEN calling to SendEmailNotification THEN SendAsync method of SMTP Client must be called `` () = async {
    // Mock SmtpClient using Foq
    let mutable sendWasCalled = false
    let mockSmtpClient = Mock<SmtpClient>()
                             .Setup(fun x -> <@ x.Connect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Disconnect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Authenticate(mockEmailConfig.SmtpUsername, mockEmailConfig.SmtpPassword) @>).Returns(())
                             .Setup(fun x -> <@ x.SendAsync(any()) @>).ReturnsFunc(fun () -> Task.Run(fun () -> sendWasCalled <- true))
                             .Create()
                             
    let bookingNotifier = BookingNotifier.provide mockEmailServiceConfiguration mockSmtpClient mockGetOffices       
       
    let! result = bookingNotifier.NotifySuccess mockBooking
    let isOk = match result with | Ok _ -> true | _ -> false
    Assert.True(isOk)
    Assert.True(sendWasCalled)
}