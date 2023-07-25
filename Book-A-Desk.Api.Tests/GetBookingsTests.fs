module Book_A_Desk.Api.Tests.GetBookingsTests


open Book_A_Desk.Domain
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events
open FsToolkit.ErrorHandling
open Newtonsoft.Json
open System
open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Cancellation.Commands
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Infrastructure.DynamoDbEventStore



let mockOfficeId =  Guid.NewGuid ()

let mockOffice =
    {
        Id = mockOfficeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = 32
        OpeningHoursText = "some opening hours"
    }

let offices = 
    mockOffice |> List.singleton

let mockGetOffices () =
    offices
    
let domainName = "domain.com"
    
let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide offices domainName
        CreateCancelBookADeskCommand = fun () -> BookADeskCancellationCommand.provide offices domainName
    }
    
let featureFlag =
    {
        BookingCancellation = true
        GetBookings = true
    }
let mockOfficeRestrictionNotification _ _ = async { return Ok [()] }

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting bookings, THEN bookings are returned`` () = async {
    let emailQuery = "unit@test.com"
    let email = emailQuery |> EmailAddress
    let date = DateTime(2100,02,01)
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    let aBooking =
        ({  
            Date = date
            EmailAddress = email 
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> [aBooking] |> Seq.ofList |> Ok |> async.Return
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore
    
    let mockEmailNotification _ = asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let! result = HttpRequest.getAsyncGetContent httpClient $"http://localhost/bookings?email={emailQuery}&date={date.ToString()}"

    let deserializedResult = JsonConvert.DeserializeObject<Bookings>(result)

    let booking = deserializedResult.Items.[0]
    let (OfficeId officeId) = officeId
    Assert.Equal(emailQuery, booking.User.Email)
    Assert.Equal(date, booking.Date)
    Assert.Equal(officeId.ToString(), booking.Office.Id)
}

[<Fact>]
let ``GIVEN A Book-A-Desk server and multiple bookings, WHEN getting bookings by Email, THEN only bookings of queried Email are returned and ordered by date`` () = async {
    let emailQuery = "unit1@test.com"
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    
    let email1 = emailQuery |> EmailAddress
    let date1 = DateTime(2020,01,01)
    let aBooking1 =
        ({  
            Date = date1
            EmailAddress = email1
            OfficeId = officeId 
        } : Events.DeskBooked) |> DeskBooked |> ReservationEvent
        
    let date2 = DateTime(2030,01,01)
    let aBooking2 =
        ({  
            Date = date2
            EmailAddress = email1 
            OfficeId = officeId 
        } : Events.DeskBooked) |> DeskBooked |> ReservationEvent
        
    let email3 = "unit2@test.com" |> EmailAddress
    let date3 = DateTime(2030,01,01)
    let aBooking3 =
        ({  
            Date = date3
            EmailAddress = email3
            OfficeId = officeId 
        } : Events.DeskBooked) |> DeskBooked |> ReservationEvent
        
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> [aBooking1; aBooking2; aBooking3] |> Seq.ofList |> Ok |> async.Return
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore
    
    let mockEmailNotification _ = asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let! result = HttpRequest.getAsyncGetContent httpClient $"http://localhost/bookings?email={emailQuery}"

    let deserializedResult = JsonConvert.DeserializeObject<Bookings>(result)
    let (OfficeId officeId) = officeId

    Assert.Equal(deserializedResult.Items.Length, 2)

    let booking1 = deserializedResult.Items.[0]
    Assert.Equal(emailQuery, booking1.User.Email)
    Assert.Equal(date1, booking1.Date)
    Assert.Equal(officeId.ToString(), booking1.Office.Id)
    
    let booking2  = deserializedResult.Items.[1]
    Assert.Equal(emailQuery, booking2.User.Email)
    Assert.Equal(date2, booking2.Date)
    Assert.Equal(officeId.ToString(), booking2.Office.Id)
}
