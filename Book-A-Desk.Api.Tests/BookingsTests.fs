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
    use httpClient = TestServer.createAndRun mockEventStore mockGetOffices
    
    let booking  = 
        {
            Office = { Id = mockOfficeId.ToString() }
            Date = DateTime.MaxValue
            User = { Email = "someEmail" }
        } : InputBooking
    
    let serializedBooking = JsonConvert.SerializeObject(booking)
    
    let! result = HttpRequest.postAsync httpClient $"http://localhost:/bookings" serializedBooking
        
    let deserializedResult = JsonConvert.DeserializeObject<Booking>(result)
    
    Assert.Equal(booking.Office.Id, deserializedResult.Office.Id)
    Assert.Equal(booking.Date, deserializedResult.Date)
    Assert.Equal(booking.User.Email, deserializedResult.User.Email)
}