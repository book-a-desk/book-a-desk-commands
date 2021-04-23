open System.Threading.Tasks
open Amazon.DynamoDBv2
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api
open Book_A_Desk.Domain
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Infrastructure
open Book_A_Desk.Infrastructure.DynamoDbEventStore

let getEvents dynamoDbEventStore eventId = async {
    let! events =  dynamoDbEventStore.GetEvents eventId
}

let provideEventStore amazonDynamoDb (provideDynamoDbEventStore : IAmazonDynamoDB -> DynamoDbEventStore) : EventStore Async =
    async {
        let dynamoDbEventStore = provideDynamoDbEventStore amazonDynamoDb
        let! getEvents eventId = dynamoDbEventStore.GetEvents eventId
        return
            {            
                GetEvents = dynamoDbEventStore.GetEvents
                AppendEvents = dynamoDbEventStore.AppendEvents
            } : EventStore
        
    }


let configureApp (app : IApplicationBuilder) =
    let eventStore amazonDynamoDb = provideEventStore DynamoDbEventStore.provide 
    let routes = Routes.provide eventStore (fun () -> Offices.All)
    app.UseGiraffe routes.HttpHandlers

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let config = serviceProvider.GetService<IConfiguration>()
    services.AddGiraffe()
            .AddDefaultAWSOptions(config.GetAWSOptions())
            .AddAWSService<IAmazonDynamoDB>() |> ignore

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0
