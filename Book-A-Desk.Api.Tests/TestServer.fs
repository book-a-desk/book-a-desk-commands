module Book_A_Desk.Api.Tests.TestServer

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api

let private configureApp eventStore getOffices (app : IApplicationBuilder) =
    let routes = Routes.provide eventStore getOffices
    app.UseGiraffe routes.HttpHandlers

let private configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore    
let createAndRun eventStore getOffices =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(configureApp eventStore getOffices)
                    .ConfigureServices(configureServices)
                    .UseUrls([| "http://localhost:5001" |])
                    |> ignore)
        .Build()
        .Run()