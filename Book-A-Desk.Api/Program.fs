open System
open Amazon
open Amazon.DynamoDBv2
open Amazon.Extensions.NETCore.Setup
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Office.Domain

let configureApp (app : IApplicationBuilder) =
    let eventStore = InMemoryEventStore.provide ()

    let getAllOffices = (fun () -> Offices.All)

    let reservationCommandsFactory = ReservationCommandsFactory.provide getAllOffices

    let apiDependencyFactory = ApiDependencyFactory.provide eventStore reservationCommandsFactory getAllOffices

    let routes = Routes.provide apiDependencyFactory
    app.UseGiraffe routes.HttpHandlers
           
let configureAppConfiguration (builder : IConfigurationBuilder) =
    let region = Environment.GetEnvironmentVariable("AWS_REGION")
    let profile = Environment.GetEnvironmentVariable("AWS_PROFILE")
    let environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
    let mutable options = AWSOptions()
    options.Region <- RegionEndpoint.GetBySystemName(region)
    options.Profile <- profile
    options.ProfilesLocation <- "/home/.aws/credentials"
    builder.AddSystemsManager($"/BookADesk/{environment}", options) |> ignore
    
let configureDynamoDB (sp : ServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    let dynamoDBConfiguration =
        {
            ReservationTableName = config.["DynamoDB:ReservationTableName"]
            OfficeTableName = config.["DynamoDB:OfficeTableName"]
        }
    Console.WriteLine(dynamoDBConfiguration.ReservationTableName)
    Console.WriteLine(dynamoDBConfiguration.OfficeTableName)
    

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
