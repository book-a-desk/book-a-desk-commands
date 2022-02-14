module Book_A_Desk.Api.Tests.FeatureFlags

open System
open System.Text.Json
open Book_A_Desk.Domain.Cancellation.Commands
open Xunit
open Book_A_Desk.Api
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Reservation.Commands
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Infrastructure

let officeId = Guid.NewGuid ()
let totalDesks = 32
let domainName = "domain.com"
let mockOffice =
    {
        Id = officeId |> OfficeId
        City = CityName "SomeCityName"
        BookableDesksPerDay = totalDesks
        OpeningHoursText = "some opening hours"
    }

let offices =
    mockOffice |> List.singleton
    
let enabledFeatureFlag =
    {
        BookingCancellation = true
    }

let disabledFeatureFlag =
    {
        BookingCancellation = false
    }

let mockReservationCommandFactory : ReservationCommandsFactory =
    {
        CreateBookADeskCommand = fun () -> BookADeskReservationCommand.provide offices domainName
        CreateCancelBookADeskCommand = fun () -> BookADeskCancellationCommand.provide offices domainName
    }

let mockEmailNotification _ = async { return Ok () }
let mockOfficeRestrictionNotification _ _ = async { return Ok [()] }

[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the flags endpoint, THEN enabled flags are returned`` () = async {
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> failwith "should not be called"
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore.DynamoDbEventStore
    let mockGetOffices () = offices
    let featureFlag = enabledFeatureFlag
    
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    let! result = HttpRequest.getAsyncGetContent httpClient "http://localhost/flags"

    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let flags = JsonSerializer.Deserialize<FeatureFlags>(result, deserializeOptions)

    Assert.True(flags.BookingCancellation)
}


[<Fact>]
let ``GIVEN A Book-A-Desk server, WHEN getting the flags endpoint, THEN disabled flags are returned`` () = async {
    let mockProvideEventStore _ =
        {
            GetEvents = fun _ -> failwith "should not be called"
            AppendEvents = fun _ -> failwith "should not be called"
        } : DynamoDbEventStore.DynamoDbEventStore
    let mockGetOffices () = offices
    let featureFlag = disabledFeatureFlag
    
    let mockApiDependencyFactory = ApiDependencyFactory.provide mockProvideEventStore mockReservationCommandFactory mockGetOffices mockEmailNotification mockOfficeRestrictionNotification featureFlag
    use httpClient = TestServer.createAndRun mockApiDependencyFactory
    let! result = HttpRequest.getAsyncGetContent httpClient "http://localhost/flags"

    let deserializeOptions = JsonSerializerOptions(JsonSerializerDefaults.Web)
    let flags = JsonSerializer.Deserialize<FeatureFlags>(result, deserializeOptions)

    Assert.False(flags.BookingCancellation)
}

