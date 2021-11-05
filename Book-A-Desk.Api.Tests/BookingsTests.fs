module Book_A_Desk.Api.Tests.BookingsTests

open Book_A_Desk.Domain.Cancellation.Commands
open FsToolkit.ErrorHandling
open System.Net
open System.Net.Http
open System.Text
open Newtonsoft.Json
open System
open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Infrastructure.DynamoDbEventStore

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

let booking  =
    {
        Office = { Id = mockOfficeId.ToString() }
        Date = DateTime.MaxValue
        User = { Email = "someEmail@domain.com" }
    } : InputBooking

let url = sprintf "http://localhost:/bookings"

let mockFeatureToggle = "True"

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN booking a desk, THEN a desk is booked`` () = async {
    let mutable emailWasSent = false 
    let mockEmailNotification _ =
        emailWasSent <- true
        asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockFeatureToggle
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let serializedBooking = JsonConvert.SerializeObject(booking)

    let! result = HttpRequest.postAsyncGetContent httpClient url serializedBooking

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
        asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockFeatureToggle
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let serializedBooking = JsonConvert.SerializeObject(booking)

    let! result = HttpRequest.postAsyncGetContent httpClient url serializedBooking
    
    Assert.True(emailWasSent)
}

[<Fact>]
let ``GIVEN an invalid reservation details WHEN booking a desk THEN it returns 400 And Reservation Error Title and Details And no notification by email is sent`` () = async {
    let mutable emailWasSent = false 
    let mockEmailNotification _ =
        emailWasSent <- true
        asyncResult { return () }
        
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockFeatureToggle
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let bookingInvalidEmail  =
        {
            booking with User = { Email = "invalidEmail" }
        } : InputBooking

    let expectedTitle = "Invalid Email Address"
    let expectedDetails = "The e-mail address is invalid."
    
    let serializedBooking = JsonConvert.SerializeObject(bookingInvalidEmail)

    let! response = HttpRequest.sendPostAsyncRequest httpClient url serializedBooking

    response
     |> (fun response ->
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        let responseObject =
            response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            |> JsonConvert.DeserializeObject<ProblemDetailsDto>
        Assert.Equal(expectedTitle, responseObject.Title)
        Assert.Equal(expectedDetails, responseObject.Details))

    Assert.False(emailWasSent)
}

[<Fact>]
let ``GIVEN an reservation WHEN notifying success fails THEN it returns 500 And error description And no notification by email is sent`` () = async {
    let error = "an error happened"
    let mockEmailNotification _ =
        Error error |> async.Return

    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockFeatureToggle
    use httpClient = TestServer.createAndRun mockApiDependencyFactory

    let serializedBooking = JsonConvert.SerializeObject(booking)

    let expectedTitle = "Generic Error"
    let expectedDetails = $"Error sending notification error for %s{booking.User.Email} at %s{booking.Date.ToShortDateString()} because {error}"

    let! response = HttpRequest.sendPostAsyncRequest httpClient url serializedBooking

    response
     |> (fun response ->
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode)
        let responseObject =
            response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            |> JsonConvert.DeserializeObject<ProblemDetailsDto>
        Assert.Equal(expectedTitle, responseObject.Title)
        Assert.Equal(expectedDetails, responseObject.Details))
}

[<Fact>]
let ``GIVEN an reservation WHEN database service fails THEN it returns 500 And Database error description And no notification by email is sent`` () = async {
    let mutable emailWasSent = false 
    let mockEmailNotification _ =
        emailWasSent <- true
        asyncResult { return () }

    let error = "Database error"

    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> Error error |> async.Return
            AppendEvents = fun _ -> () |> async.Return
        } : DynamoDbEventStore

    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockFeatureToggle
    use httpClient = TestServer.createAndRun mockApiDependencyFactory

    let serializedBooking = JsonConvert.SerializeObject(booking)

    let expectedTitle = "Generic Error"
    let expectedDetails = error

    let! response = HttpRequest.sendPostAsyncRequest httpClient url serializedBooking

    response
    |> (fun response ->
        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode)
        let responseObject =
            response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            |> JsonConvert.DeserializeObject<ProblemDetailsDto>
        Assert.Equal(expectedTitle, responseObject.Title)
        Assert.Equal(expectedDetails, responseObject.Details))

    Assert.False(emailWasSent)
}