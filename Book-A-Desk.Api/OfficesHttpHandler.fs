namespace Book_A_Desk.Api

open Amazon.DynamoDBv2
open Book_A_Desk.Domain.Reservation
open Book_A_Desk.Domain.Reservation.Domain
open Book_A_Desk.Infrastructure.DynamoDbEventStore
open FSharp.Control.Tasks.V2.ContextInsensitive
open FsToolkit.ErrorHandling
open Giraffe
open System

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Domain.Office.Queries
open Book_A_Desk.Domain.Reservation.Queries
open Book_A_Desk.Domain.QueriesHandler

type OfficesHttpHandler =
    {
        HandleGetAll: unit -> HttpHandler
        HandleGetByDate: Guid -> HttpHandler
    }

module rec OfficesHttpHandler =
    let initialize eventStore getOffices =
        {
            HandleGetAll = fun () -> handleGet getOffices
            HandleGetByDate = handleGetByDate eventStore getOffices
        }

    let handleGet getOffices = fun next context ->
        task {
            let result = OfficeQueriesHandler.getAll getOffices
            match result with
            | Ok offices ->
                let offices =
                    offices
                    |> List.map (fun (o:Office) ->
                        let (OfficeId officeId) = o.Id
                        let (CityName cityName) = o.City
                        {
                            Id = officeId.ToString()
                            Name = cityName
                        })
                    |> List.toArray
                    |> fun l -> { Offices.Items = l }
                return! json offices next context
            | Error e ->
                context.SetStatusCode(500)
                return! text ("Internal Error: " + e) next context
        }

    let handleGetByDate (provideEventStore : IAmazonDynamoDB -> DynamoDbEventStore) getOffices officeId = fun next context ->
        task {
            let date = InputParser.parseDateFromContext context

            match date with
            | None ->
                context.SetStatusCode(400)
                return! text "Date could not be parsed" next context
            | Some date ->
                let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())
                let! result = handleGetByDateFromEventStore eventStore getOffices date officeId
                
                match result with
                | Ok officeAvailability ->
                    let (OfficeId officeId) = officeAvailability.Id
                    let officeAvailability =
                        {
                            Id = officeId.ToString()
                            TotalDesks = officeAvailability.TotalDesks
                            AvailableDesks = officeAvailability.AvailableDesks
                        } : Book_A_Desk.Api.Models.OfficeAvailability
                    return! json officeAvailability next context
                | Error e ->
                    context.SetStatusCode(500)
                    return! text ("Internal Error: " + e) next context
        }
        
    let private handleGetByDateFromEventStore eventStore getOffices date officeId = asyncResult {
        let (ReservationId aggregateId) = ReservationAggregate.Id
        let! bookingEvents = eventStore.GetEvents aggregateId
        let getBookingsForDate = ReservationsQueriesHandler.get bookingEvents
        let query =
            {
                OfficeId = officeId |> OfficeId
                Date = date
            }

        return! OfficeQueriesHandler.getAvailabilities getOffices getBookingsForDate query
    }
