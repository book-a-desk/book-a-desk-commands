module Book_A_Desk.Api.Tests.BookingsTests

open Book_A_Desk.Domain.Reservation.Commands
open Newtonsoft.Json
open System
open Xunit
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain

let mockEventStore =
    {
        GetEvents = fun _ -> Result.Ok(List.empty)
        AppendEvents = fun _ -> ()
    }
 
let mockOfficeId =  Guid.NewGuid ()
    
let mockOffice =
    {
        Id = mockOfficeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = 32
    } 
    
let mockGetOffices () =
    mockOffice |> List.singleton

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN booking a desk, THEN a desk is booked`` () = async {
    let port = 5002
    async { TestServer.createAndRun port mockEventStore mockGetOffices } |> Async.Start
    
    let booking  = 
        {
            Office = { Id = mockOfficeId.ToString() }
            Date = DateTime.MaxValue
            User = { Email = "someEmail" }
        }
    
    let serializedBooking = JsonConvert.SerializeObject(booking)
    
    use httpClient = new System.Net.Http.HttpClient()
    let! result = HttpRequest.postAsync httpClient $"http://localhost:{port}/bookings" serializedBooking
        
    let deserializedResult = JsonConvert.DeserializeObject<BookADesk>(result)
    
    let (OfficeId officeId) = deserializedResult.OfficeId
    let (EmailAddress emailAddress) = deserializedResult.EmailAddress
    Assert.Equal(booking.Office.Id, officeId.ToString())
    Assert.Equal(booking.Date, deserializedResult.Date)
    Assert.Equal(booking.User.Email, emailAddress)
}