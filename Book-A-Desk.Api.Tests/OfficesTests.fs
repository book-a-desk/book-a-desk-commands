module Book_A_Desk.Api.Tests.Offices

open System
open System.Text.Json

open Book_A_Desk.Domain.QueriesHandler
open Xunit

open Book_A_Desk.Api
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Events
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Events
open Book_A_Desk.Infrastructure

let officeId = Guid.NewGuid ()
let totalDesks = 32
let domainName = "domain.com"
let mockOffice =
    {
        Id = officeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = totalDesks
    }

let offices =
    mockOffice |> List.singleton

let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide offices domainName
    }

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the offices endpoint, THEN offices are returned`` () = async {
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> failwith "should not be called"
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore.DynamoDbEventStore
    let mockEmailNotification booking = async { return true }
    let mockGetOffices () = offices
    
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    let! result = HttpRequest.getAsync httpClient "http://localhost/offices"

    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let offices = JsonSerializer.Deserialize<Book_A_Desk.Api.Models.Offices>(result, deserializeOptions)

    Assert.Equal(1, offices.Items.Length)
    let office = offices.Items.[0]
    let (OfficeId id) = mockOffice.Id
    let (CityName cityName) = mockOffice.City
    Assert.Equal(id.ToString(), office.Id)
    Assert.Equal(cityName, office.Name)
}

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the office availability by date, THEN office availability is returned`` () = async {
    let date = DateTime(2100,02,01)
    let aBooking =
        {
            ReservationId = ReservationAggregate.Id
            Date = date
            EmailAddress = "anEmail" |> EmailAddress
            OfficeId = officeId |> OfficeId
        } |> ReservationEvent.DeskBooked |> ReservationEvent
    let mockGetOffices () = offices

    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> [aBooking] |> Seq.ofList |> Ok |> async.Return
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore.DynamoDbEventStore
        
    let mockEmailNotification _ = async { return true }    
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification
    use httpClient = TestServer.createAndRun mockApiDependencyFactory

    let! result = HttpRequest.getAsync httpClient $"http://localhost/offices/{officeId.ToString()}/availabilities?date={date.ToString()}"

    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let officeAvailability = JsonSerializer.Deserialize<Book_A_Desk.Api.Models.OfficeAvailability>(result, deserializeOptions)

    let (OfficeId id) = mockOffice.Id
    Assert.Equal(id.ToString(), officeAvailability.Id)
    Assert.Equal(totalDesks, officeAvailability.TotalDesks)
    Assert.Equal(totalDesks - 1, officeAvailability.AvailableDesks)
}

[<Fact>]
let ``GIVEN an existing Office with OfficeId WHEN getting the office name by OfficeId THEN City Name is SomeCityName`` () =
    let expectedOfficeName = mockOffice.City    
    let mockGetOffices () = offices
    
    let result = OfficeQueriesHandler.getOfficeNameById mockOffice.Id mockGetOffices
    Assert.Equal(expectedOfficeName, result)

[<Fact>]
let ``GIVEN an invalid OfficeId WHEN getting the office name by OfficeId THEN City Name is Unknown`` () =
    let expectedOfficeName = "Unknown" |> CityName
    let invalidOfficeId = Guid.NewGuid () |> OfficeId
    let mockGetOffices () = offices
        
    let result = OfficeQueriesHandler.getOfficeNameById invalidOfficeId mockGetOffices
    Assert.Equal(expectedOfficeName, result)
