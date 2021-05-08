open System
open Amazon
open Amazon.DynamoDBv2
open Amazon.Extensions.NETCore.Setup
open Amazon.Runtime
open Book_A_Desk.Domain.Office.Domain
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api
open Book_A_Desk.Domain

let configureApp (app : IApplicationBuilder) =
    let eventStore = InMemoryEventStore.provide ()
    let routes = Routes.provide eventStore (fun () -> Offices.All)
    app.UseGiraffe routes.HttpHandlers
           
let configureAppConfiguration (builder : IConfigurationBuilder) =
    let region = Environment.GetEnvironmentVariable("AWS_REGION")
//    let profile = Environment.GetEnvironmentVariable("AWS_PROFILE")
    let keyID = Environment.GetEnvironmentVariable("AWS_KEY_ID")
    let secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY")
    let credentials = BasicAWSCredentials(keyID, secretKey)
    let mutable options = AWSOptions()
    options.Region <- RegionEndpoint.GetBySystemName(region)
    options.Credentials <- credentials
    builder.AddSystemsManager("/BookADesk", options) |> ignore
    
let configureDynamoDB (sp : ServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    let dynamoDBConfiguration =
        {
            TableName = config.["DynamoDB:TableName"]
        }
    Console.WriteLine(dynamoDBConfiguration.TableName)
    

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let config = serviceProvider.GetService<IConfiguration>()
    services.AddGiraffe()
            .AddDefaultAWSOptions(config.GetAWSOptions())
            .AddAWSService<IAmazonDynamoDB>() |> ignore
    configureDynamoDB serviceProvider

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp)
                    .ConfigureAppConfiguration(Action<IConfigurationBuilder> configureAppConfiguration)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0
