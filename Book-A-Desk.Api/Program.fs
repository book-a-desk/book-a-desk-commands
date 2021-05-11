open Amazon.DynamoDBv2
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
