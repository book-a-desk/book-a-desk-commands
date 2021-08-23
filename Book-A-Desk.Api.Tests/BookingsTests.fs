module Book_A_Desk.Api.Tests.BookingsTests

open Book_A_Desk.Infrastructure.DynamoDbEventStore
open Newtonsoft.Json
open System
open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Office.Domain

let mockProvideEventStore _ =
    {
        GetEvents = fun _ -> Seq.empty |> Ok |> async.Return
        AppendEvents = fun _ -> () |> async.Return
    } : DynamoDbEventStore

let mockOfficeId =  Guid.NewGuid ()

let mockOffice =
    {
        Id = mockOfficeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = 32
        OpeningHoursText = None
    }

let offices = 
    mockOffice |> List.singleton

let mockGetOffices () =
    offices
    
let domainName = "domain.com"
    
let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide offices domainName
    }
    
let booking  =
    {
        Office = { Id = mockOfficeId.ToString() }
        Date = DateTime.MaxValue
        User = { Email = "someEmail" }
    } : InputBooking

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN booking a desk, THEN a desk is booked`` () = async {
    let mockEmailNotification _ = async { return Ok () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let serializedBooking = JsonConvert.SerializeObject(booking)

    let! result = HttpRequest.postAsync httpClient $"http://localhost:/bookings" serializedBooking

    let deserializedResult = JsonConvert.DeserializeObject<Booking>(result)

    Assert.Equal(booking.Office.Id, deserializedResult.Office.Id)
    Assert.Equal(booking.Date, deserializedResult.Date)
    Assert.Equal(booking.User.Email, deserializedResult.User.Email)
}

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN booking a desk, THEN an email notification must be sent`` () = async {
    let mutable emailWasSent = false
    let mockEmailNotification _ =
        emailWasSent <- true
        async { return Ok () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let serializedBooking = JsonConvert.SerializeObject(booking)

    do! HttpRequest.postAsync httpClient $"http://localhost:/bookings" serializedBooking |> Async.Ignore
    
    Assert.True(emailWasSent)
}
