
open Book_A_Desk.Domain.CommandHandler
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Domain

open Book_A_Desk.Api

let configureApp (app : IApplicationBuilder) =
    let eventStore = InMemoryEventStore.provide ()
    let reservationCommandsFactory = ReservationCommandsFactory.provide ()

    let routes = Routes.provide eventStore reservationCommandsFactory
    app.UseGiraffe routes.HttpHandlers

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
