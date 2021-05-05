module Book_A_Desk.Api.Tests.BookingsTests

open Newtonsoft.Json
open System
open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Office.Domain

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

let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide mockGetOffices
    }

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN booking a desk, THEN a desk is booked`` () = async {
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockEventStore mockReservationCommandFactory mockGetOffices
    use httpClient = TestServer.createAndRun mockApiDependencyFactory

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
