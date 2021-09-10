module Book_A_Desk.Api.Tests.BookingsTests

open System.Net
open System.Net.Http
open System.Text
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
    }

let offices = 
    mockOffice |> List.singleton

let mockGetOffices () =
    offices
    
let domainName = "broadsign.com"
    
let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide offices domainName
    }
    
let mutable emailWasSent = false 
let mockEmailNotification booking =
    emailWasSent <- true
    async { return emailWasSent }  

let booking  =
    {
        Office = { Id = mockOfficeId.ToString() }
        Date = DateTime.MaxValue
        User = { Email = "someEmail@broadsign.com" }
    } : InputBooking

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN booking a desk, THEN a desk is booked`` () = async {
    emailWasSent <- false
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
    emailWasSent <- false
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let serializedBooking = JsonConvert.SerializeObject(booking)

    let! result = HttpRequest.postAsync httpClient $"http://localhost:/bookings" serializedBooking
    
    Assert.True(emailWasSent)
}

[<Fact>]
let ``GIVEN an invalid reservation details WHEN booking a desk THEN it returns 400 And Reservation Error Title and Details And no notification by email is sent`` () = async {
    emailWasSent <- false
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    
    let bookingInvalidEmail  =
        {
            booking with User = { Email = "invalidEmail" }
        } : InputBooking
    
    let serializedBooking = JsonConvert.SerializeObject(bookingInvalidEmail)

    let url = sprintf "http://localhost:/bookings"

    let postRequest = new HttpRequestMessage(HttpMethod.Post, url)
    let content = new StringContent(serializedBooking, Encoding.UTF8, "application/json")
    postRequest.Content <- content

    httpClient.SendAsync postRequest |> Async.AwaitTask |> Async.RunSynchronously
     |> (fun response ->
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode)
        let responseObject =
            response.Content.ReadAsStringAsync() |> Async.AwaitTask |> Async.RunSynchronously
            |> JsonConvert.DeserializeObject<ProblemDetailsDto>
        Assert.Equal("Invalid Email Address", responseObject.Title)
        Assert.Equal("The e-mail address is invalid.", responseObject.Detail))

    Assert.False(emailWasSent)
}