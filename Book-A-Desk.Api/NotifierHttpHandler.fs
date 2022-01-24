namespace Book_A_Desk.Api

open Amazon.DynamoDBv2
open Book_A_Desk.Infrastructure.DynamoDbEventStore
open Microsoft.AspNetCore.Http
open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Api.Models


type NotifierHttpHandler =
    {
        HandlePostWith: RestrictionNotifier -> HttpHandler
    }

module NotifierHttpHandler =
    let initialize
        (provideEventStore: IAmazonDynamoDB -> DynamoDbEventStore)
        (notifyOfficeRestrictions: DynamoDbEventStore -> RestrictionNotifier -> Async<Result<unit list, string>>)
        errorHandler =

        let handlePostWith (restrictionNotifier: RestrictionNotifier) = fun next (context : HttpContext) ->
            task {
                let eventStore = provideEventStore (context.GetService<IAmazonDynamoDB>())

                let! result = notifyOfficeRestrictions eventStore restrictionNotifier
                match result with
                | Ok _ ->
                    context.SetStatusCode(StatusCodes.Status200OK)
                    return! next context
                | Error (response: string) ->
                    let  error = $"Error sending Office Restriction notifications error %s{response}"
                    let responseError = (errorHandler.MapStringToAssignBookADeskError error)
                                        |> errorHandler.ConvertErrorToResponseError
                    context.SetStatusCode(StatusCodes.Status500InternalServerError)
                    return! json responseError next context
            }

        {
            HandlePostWith = handlePostWith
        }
