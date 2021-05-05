module Book_A_Desk.Api.Tests.TestServer

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api

let private configureApp apiDependencyFactory (app : IApplicationBuilder) =
    let routes = Routes.provide apiDependencyFactory
    app.UseGiraffe routes.HttpHandlers

let private configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

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
