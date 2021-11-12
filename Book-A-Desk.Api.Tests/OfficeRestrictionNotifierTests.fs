module Book_A_Desk.Api.Tests.OfficeRestrictionNotifier

open System
open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Infrastructure.DynamoDbEventStore
open FSharp.Control.Tasks
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
        EmailSender = "from@domain.com"
        EmailReviewer = "reviewer@domain.com"
    }

let mockEmailServiceConfiguration () = mockEmailConfig

let mockProvideEventStore =
    {
        GetEvents = fun _ -> Seq.empty |> Ok |> async.Return
        AppendEvents = fun _ -> () |> async.Return
    } : DynamoDbEventStore

let mockBookingNotifier =
    {
        NotifySuccess = fun _ ->  () |> Ok |> async.Return
        NotifyOfficeRestrictionToBooking = fun (booking : Models.Booking) ->  () |> Ok |> async.Return
    } : BookingNotifier

[<Fact>]
let ``GIVEN a booking for the following day WHEN Office opening time happens THEN an email with office restrictions is sent to bookings email account `` () = async {
    // Mock SmtpClient using Foq
    let mutable sendWasCalled = false
    let mockSmtpClient = Mock<SmtpClient>()
                             .Setup(fun x -> <@ x.Connect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Disconnect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Authenticate(mockEmailConfig.SmtpUsername, mockEmailConfig.SmtpPassword) @>).Returns(())
                             .Setup(fun x -> <@ x.SendAsync(any()) @>).Returns(task {sendWasCalled <- true})
                             .Create()

    let getBookings _ = Ok [mockBooking]

    let officeRestrictionNotifier = OfficeRestrictionNotifier.provide mockBookingNotifier.NotifyOfficeRestrictionToBooking mockProvideEventStore mockGetOffices

    officeRestrictionNotifier.Execute date |> Async.RunSynchronously
    Assert.True(sendWasCalled)
}

[<Fact>]
let ``GIVEN no bookings for the following day WHEN Office opening time happens THEN no email is sent`` () = async {
    // Mock SmtpClient using Foq
    let mutable sendWasCalled = false
    let mockSmtpClient = Mock<SmtpClient>()
                             .Setup(fun x -> <@ x.Connect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Disconnect(any(), any()) @>).Returns(())
                             .Setup(fun x -> <@ x.Authenticate(mockEmailConfig.SmtpUsername, mockEmailConfig.SmtpPassword) @>).Returns(())
                             .Setup(fun x -> <@ x.SendAsync(any()) @>).Returns(task {sendWasCalled <- true})
                             .Create()

    let bookingNotifier = BookingNotifier.provide mockEmailServiceConfiguration mockSmtpClient mockGetOffices

    let getBookings _ = Ok []

    let officeRestrictionNotifier = OfficeRestrictionNotifier.provide mockBookingNotifier.NotifyOfficeRestrictionToBooking mockProvideEventStore mockGetOffices

    officeRestrictionNotifier.Execute date |> Async.RunSynchronously
    Assert.False(sendWasCalled)
}

