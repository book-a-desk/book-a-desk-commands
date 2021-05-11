module Book_A_Desk.Api.Tests.Offices

open System
open System.Text.Json

open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events

let officeId = Guid.NewGuid ()
let totalDesks = 32
let mockOffice =
    {
        Id = officeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = totalDesks
    }

let mockGetOffices () =
    mockOffice |> List.singleton

let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide mockGetOffices
    }

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the offices endpoint, THEN offices are returned`` () = async {
    let mockEventStore =
        {
            GetEvents = fun _ -> failwith "should not be called"
            AppendEvents = fun _ -> failwith "should not be called"
        }

    let mockApiDependencyFactory = ApiDependencyFactory.provide mockEventStore mockReservationCommandFactory mockGetOffices
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    let! result = HttpRequest.getAsync httpClient "http://localhost/offices"

    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let offices = JsonSerializer.Deserialize<Book_A_Desk.Api.Models.Office array>(result, deserializeOptions)

    Assert.Equal(1, offices.Length)
    let office = offices.[0]
    let (OfficeId id) = mockOffice.Id
    let (CityName cityName) = mockOffice.City
    Assert.Equal(id.ToString(), office.Id)
    Assert.Equal(cityName, office.Name)
}

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the office availability by date, THEN office availability is returned`` () = async {
    let date = DateTime(2021,02,01)
    let aBooking =
        {
            ReservationId = ReservationAggregate.Id
            Date = date
            EmailAddress = "anEmail" |> EmailAddress
            OfficeId = officeId |> OfficeId
        } |> DeskBooked |> ReservationEvent

    let mockEventStore =
        {
            GetEvents = fun _ -> Ok [aBooking]
            AppendEvents = fun _ -> failwith "should not be called"
        }
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockEventStore mockReservationCommandFactory mockGetOffices
    use httpClient = TestServer.createAndRun mockApiDependencyFactory

    let! result = HttpRequest.getAsync httpClient $"http://localhost/offices/{officeId.ToString()}/availabilities?date={date.ToString()}"

    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let officeAvailability = JsonSerializer.Deserialize<Book_A_Desk.Api.Models.OfficeAvailability>(result, deserializeOptions)

    let (OfficeId id) = mockOffice.Id
    Assert.Equal(id.ToString(), officeAvailability.Id)
    Assert.Equal(totalDesks, officeAvailability.TotalDesks)
    Assert.Equal(totalDesks - 1, officeAvailability.AvailableDesks)
}
