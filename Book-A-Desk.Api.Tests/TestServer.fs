module Book_A_Desk.Api.Tests.TestServer

open Amazon.DynamoDBv2
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api
open Book_A_Desk.Api.Tests

let private configureApp apiDependencyFactory (app : IApplicationBuilder) =
    let routes = Routes.provide apiDependencyFactory
    app.UseGiraffe routes.HttpHandlers

let private configureServices (services : IServiceCollection) =
    let mockDynamoDb = new MockAmazonDynamoDB()
    let mockEmailService = MockEmailServiceConfiguration()
    services.AddGiraffe() |> ignore
    services.AddSingleton<IAmazonDynamoDB>(mockDynamoDb) |> ignore
    services.AddSingleton<IConfiguration>(mockEmailService) |> ignore

let createAndRun apiDependencyFactory =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp apiDependencyFactory)
                    .ConfigureServices(configureServices)
                    .UseTestServer()
                    |> ignore
                )
        .Start()
        .GetTestClient()
