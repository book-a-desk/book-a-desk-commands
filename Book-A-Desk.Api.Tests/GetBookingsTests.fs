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
    let dateQuery = DateTime(2030,02,01)
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    let aBooking =
        ({  
            ReservationId = ReservationAggregate.Id
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
    
    let! result = HttpRequest.getAsyncGetContent httpClient $"http://localhost/bookings?email={emailQuery}&date={dateQuery.ToString()}"

    let deserializedResult = JsonConvert.DeserializeObject<Bookings>(result)

    let booking = deserializedResult.Items.[0]
    let (OfficeId officeId) = officeId
    Assert.Equal(emailQuery, booking.User.Email)
    Assert.Equal(date, booking.Date)
    Assert.Equal(officeId.ToString(), booking.Office.Id)
}

[<Fact>]
let ``GIVEN A Book-A-Desk server and multiple bookings, WHEN getting bookings, THEN only bookings of queried user and future date are returned`` () = async {
    let emailQuery = "unit1@test.com"
    let dateQuery = DateTime(2030,02,01)
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    
    let email1 = emailQuery |> EmailAddress
    let date1 = DateTime(2100,02,01)
    let aBooking1 =
        ({  
            ReservationId = ReservationAggregate.Id
            Date = date1
            EmailAddress = email1
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let email2 = "unit2@test.com" |> EmailAddress
    let date2 = DateTime(2100,02,01)
    let aBooking2 =
        ({  
            ReservationId = ReservationAggregate.Id
            Date = date2
            EmailAddress = email2 
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let date3 = DateTime(2030,01,30)
    let aBooking3 =
        ({  
            ReservationId = ReservationAggregate.Id
            Date = date3
            EmailAddress = email1
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> [aBooking1; aBooking2; aBooking3] |> Seq.ofList |> Ok |> async.Return
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore
    
    let mockEmailNotification _ = asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let! result = HttpRequest.getAsyncGetContent httpClient $"http://localhost/bookings?email={emailQuery}&date={dateQuery.ToString()}"

    let deserializedResult = JsonConvert.DeserializeObject<Bookings>(result)

    let booking = deserializedResult.Items.[0]
    let (OfficeId officeId) = officeId
    Assert.Equal(emailQuery, booking.User.Email)
    Assert.Equal(date1, booking.Date)
    Assert.Equal(officeId.ToString(), booking.Office.Id)
}

[<Fact>]
let ``GIVEN A Book-A-Desk server and multiple bookings, WHEN getting bookings without email provided, THEN all bookings of future date are returned`` () = async {
    let emailQuery = "email1@test.com"
    let dateQuery = DateTime(2030,02,01)
    let officeId = Guid.Parse("16C3D468-C115-4452-8502-58B821D6640B") |> OfficeId
    
    let email1 = emailQuery |> EmailAddress
    let date1 = DateTime(2100, 01, 20)
    let aBooking1 =
        ({  
            ReservationId = ReservationAggregate.Id
            Date = date1
            EmailAddress = email1
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let email2 = "email2@test.com" |> EmailAddress
    let date2 = DateTime(2100, 01, 21)
    let aBooking2 =
        ({  
            ReservationId = ReservationAggregate.Id
            Date = date2
            EmailAddress = email2 
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let date3 = DateTime(2100, 01, 22)
    let aBooking3 =
        ({  
            ReservationId = ReservationAggregate.Id
            Date = date3
            EmailAddress = email2
            OfficeId = officeId 
        } : Events.DeskBooked) |> ReservationEvent.DeskBooked |> ReservationEvent
        
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> [aBooking1; aBooking2; aBooking3] |> Seq.ofList |> Ok |> async.Return
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore
    
    let mockEmailNotification _ = asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let! result = HttpRequest.getAsyncGetContent httpClient $"http://localhost/bookings?date={dateQuery.ToString()}"

    let deserializedResult = JsonConvert.DeserializeObject<Bookings>(result)

    let bookings = deserializedResult.Items
    Assert.Equal(3, bookings.Length)
}
