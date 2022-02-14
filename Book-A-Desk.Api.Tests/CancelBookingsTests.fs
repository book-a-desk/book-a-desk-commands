module Book_A_Desk.Api.Tests.CancelBookingsTests

open FsToolkit.ErrorHandling
open System.Net
open Newtonsoft.Json
open System
open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Cancellation.Commands
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Domain.Reservation.Events
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
    
let emailAddress = "someEmail@domain.com"
let bookingDate = DateTime.Today.Add(TimeSpan.FromDays(1.0))
let cancellation  =
        {
            Office = { Id = mockOfficeId.ToString() }
            Date = bookingDate
            User = { Email = emailAddress }
        } : InputCancellation
    
let domainName = "domain.com"
    
let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide offices domainName
        CreateCancelBookADeskCommand = fun () -> BookADeskCancellationCommand.provide offices domainName
    }

let bookingsUrl = sprintf "http://localhost:/bookings"
let cancelBookingsUrl = sprintf "http://localhost:/cancelBookings"

let featureFlag =
    {
        BookingCancellation = true
    }
let mockEmailNotification _ = asyncResult { return () }
let mockOfficeRestrictionNotification _ _ = async { return Ok [()] }

[<Fact>]
let ``GIVEN A Book-A-Desk server and a booking, WHEN cancelling a desk, THEN a desk is cancelled`` () = async {
    let event =
        {
            DeskBooked.ReservationId = ReservationAggregate.Id
            Date = bookingDate
            EmailAddress = emailAddress |> EmailAddress
            OfficeId = mockOfficeId |> OfficeId
        } |> DeskBooked |> ReservationEvent
    
    let events = [event] |> Seq.ofList
    let mutable receivedEvents = Seq.empty

    let mockProvideEventStore _ =
        {
            GetEvents = fun _ ->
                events |> Ok |> async.Return
            AppendEvents = fun event ->
                let (ReservationId reservationID) = ReservationAggregate.Id
                receivedEvents <- event.[reservationID]
                () |> async.Return
        } : DynamoDbEventStore
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let serializedCancellation = JsonConvert.SerializeObject(cancellation)
    
    do! HttpRequest.postAsyncGetContent httpClient cancelBookingsUrl serializedCancellation |> Async.Ignore
    
    let receivedEvent = Assert.Single receivedEvents
                        |> function | ReservationEvent event -> event
    match receivedEvent with
    | DeskBooked _ ->
        failwith "should not be a desk booked"
    | DeskCancelled event ->
        Assert.Equal(cancellation.Date, event.Date)
        let (OfficeId officeId) = event.OfficeId
        Assert.Equal(cancellation.Office.Id, officeId.ToString())
        let (EmailAddress email) = event.EmailAddress
        Assert.Equal(cancellation.User.Email, email)
}

[<Fact>]
let ``GIVEN an invalid reservation details WHEN cancelling a desk THEN it returns 400 And Reservation Error Title and Details And no notification by email is sent`` () = async {
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> Seq.empty |> Ok |> async.Return
            AppendEvents = fun _ -> () |> async.Return
        } : DynamoDbEventStore
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let bookingInvalidEmail  =
        {
            cancellation with User = { Email = "invalidEmail" }
        } : InputCancellation

    let expectedTitle = "Invalid Email Address"
    let expectedDetails = "The e-mail address is invalid."
    
    let serializedBooking = JsonConvert.SerializeObject(bookingInvalidEmail)

    let! response = HttpRequest.sendPostAsyncRequest httpClient cancelBookingsUrl serializedBooking

    response
     |> (fun response ->
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        let responseObject =
            response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            |> JsonConvert.DeserializeObject<ProblemDetailsDto>
        Assert.Equal(expectedTitle, responseObject.Title)
        Assert.Equal(expectedDetails, responseObject.Details))
}

[<Fact>]
let ``GIVEN an reservation WHEN database service fails THEN it returns 500 And Database error description`` () = async {
    let error = "Database error"

    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> Error error |> async.Return
            AppendEvents = fun _ -> () |> async.Return
        } : DynamoDbEventStore

    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory

    let serializedCancellation = JsonConvert.SerializeObject(cancellation)

    let expectedTitle = "Generic Error"
    let expectedDetails = error

    let! response = HttpRequest.sendPostAsyncRequest httpClient cancelBookingsUrl serializedCancellation

    response
    |> (fun response ->
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode)
        let responseObject =
            response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            |> JsonConvert.DeserializeObject<ProblemDetailsDto>
        Assert.Equal(expectedTitle, responseObject.Title)
        Assert.Equal(expectedDetails, responseObject.Details))
}