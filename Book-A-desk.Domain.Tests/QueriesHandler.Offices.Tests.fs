module Book_A_desk.Domain.Office.Queries.Tests

open System
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Reservation
open Xunit

open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Office.Queries
open Book_A_Desk.Domain.QueriesHandler

[<Fact>]
let ``GIVEN A OfficeQueriesHandler, WHEN getting the offices, THEN offices are returned`` () =
    let anOffice = {
        Id = OfficeId (Guid.NewGuid())
        City = CityName "A City"
        BookableDesksPerDay = 2
    }
    let getOffices () = [anOffice]   
    
    let result = OfficeQueriesHandler.getAll getOffices
    
    match result with
    | Error _ -> failwith "offices should have been returned"
    | Ok [office] ->
        Assert.Equal(anOffice, office)
    | Ok _ -> failwith "offices should have been the one given"
    
[<Fact>]
let ``GIVEN A OfficeQueriesHandler WITH an invalid office id, WHEN getting the availabilities, THEN an error is returned`` () =
    let getOffices () = []
    let getBookingsCountPerDate _ = Ok []
    let query =
            {
                OfficeId = OfficeId (Guid.NewGuid())
                Date = DateTime.MaxValue
            } : GetOfficeAvailabilitiesByDate
    
    let result = OfficeQueriesHandler.getAvailabilities getOffices getBookingsCountPerDate query
    
    match result with
    | Ok _ -> failwith "there should have been an error"
    | Error _ -> ()
    
[<Fact>]
let ``GIVEN A OfficeQueriesHandler, WHEN getting the availabilities, THEN availabilities are returned`` () =
    let officeId = OfficeId (Guid.NewGuid())
    let availableDesks = 2
    let anOffice = {
        Id = officeId
        City = CityName "A City"
        BookableDesksPerDay = availableDesks
    }
    let getOffices () = [anOffice]
    
    let aBooking =
        {
            OfficeId = officeId
            EmailAddress = EmailAddress "anEmail"
            Date = DateTime.MaxValue
        } : Booking
    
    let getBookingsCountPerDate _ = Ok [aBooking]    
    
    let query =
            {
                OfficeId = officeId
                Date = DateTime.MaxValue
            } : GetOfficeAvailabilitiesByDate
    
    let result = OfficeQueriesHandler.getAvailabilities getOffices getBookingsCountPerDate query
    
    match result with
    | Error _ -> failwith "something should have been returned"
    | Ok avail ->
        Assert.Equal(officeId, avail.Id)
        Assert.Equal(availableDesks, avail.TotalDesks)
        Assert.Equal(availableDesks - 1, avail.AvailableDesks)