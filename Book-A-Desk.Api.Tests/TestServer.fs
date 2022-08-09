module Book_A_Desk.Api.Tests.TestServer

open System.Threading.Tasks
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

let mockValidateToken _ = ValidToken |> Task.FromResult

let private configureApp apiDependencyFactory (app : IApplicationBuilder) =
    let routes = Routes.provide apiDependencyFactory mockValidateToken
    app.UseGiraffe routes.HttpHandlers

let private configureServices (services : IServiceCollection) =
    let mockDynamoDb = new MockAmazonDynamoDB()
    let mockEmailService = MockEmailServiceConfiguration()
    services.AddGiraffe() |> ignore
    services.AddSingleton<IAmazonDynamoDB>(mockDynamoDb) |> ignore
    services.AddSingleton<IConfiguration>(mockEmailService) |> ignore

// Default bearer token from https://jwt.io/
let bearerToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiIxMjM0NTY3ODkwIiwibmFtZSI6IkpvaG4gRG9lIiwiaWF0IjoxNTE2MjM5MDIyfQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c"

let createAndRun apiDependencyFactory =
    let testClient =
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
    testClient.DefaultRequestHeaders.Add("Authorization", bearerToken)
    testClient
