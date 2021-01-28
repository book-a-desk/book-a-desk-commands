module Book_A_Desk.Api.Tests.Offices

open System
open System.Text.Json
open Book_A_Desk.Domain.Office.Domain
open Xunit
open Book_A_Desk.Domain

let mockEventStore =
    {
        GetEvents = fun _ -> failwith "should not be called"
        AppendEvents = fun _ -> failwith "should not be called"
    }
    
let mockOffice =
    {
        Id = Guid.NewGuid () |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = 32
    } 
    
let mockGetOffices () =
    mockOffice |> List.singleton

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the offices endpoint, THEN offices are returned`` () = async {    
    use httpClient = TestServer.createAndRun mockEventStore mockGetOffices
    let! result = HttpRequest.getAsync httpClient $"http://localhost/offices"
    
    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let offices = JsonSerializer.Deserialize<Book_A_Desk.Api.Models.Office array>(result, deserializeOptions)
    
    Assert.Equal(1, offices.Length)
    let office = offices.[0]
    let (OfficeId id) = mockOffice.Id
    let (CityName cityName) = mockOffice.City
    Assert.Equal(id.ToString(), office.Id)
    Assert.Equal(cityName, office.Name)
}