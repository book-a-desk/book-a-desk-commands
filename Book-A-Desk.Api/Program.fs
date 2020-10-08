open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open BookADesk.Api.HttpHandlers

let webApp =
    choose [
        route "/hello" >=> handleGetHello ]

let configureApp (app : IApplicationBuilder) =
    app.UseGiraffe webApp

let configureServices (services : IServiceCollection) =
    services.AddGiraffe() |> ignore

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
