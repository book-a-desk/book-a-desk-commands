open System
open Amazon
open Amazon.DynamoDBv2
open Amazon.Extensions.NETCore.Setup
open Amazon.Runtime
open Book_A_Desk.Api.Models
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api
open Book_A_Desk.Domain
open Book_A_Desk.Domain.CommandHandler
open Book_A_Desk.Domain.Office.Domain

 
let configureCors (ctx : WebHostBuilderContext) (builder : CorsPolicyBuilder) =
    if ctx.HostingEnvironment.IsDevelopment() then
        builder
           .AllowAnyOrigin()
           .AllowAnyMethod()
           .AllowAnyHeader()
           |> ignore
    else
        builder
            .WithOrigins(ctx.Configuration.["Book-A-Desk-Frontend:Url"])
            .AllowAnyMethod()
            .AllowAnyHeader()
            |> ignore

let configureApp (ctx : WebHostBuilderContext) (app : IApplicationBuilder) =
    let eventStore = InMemoryEventStore.provide ()

    let getAllOffices = (fun () -> Offices.All)

    let reservationCommandsFactory = ReservationCommandsFactory.provide getAllOffices

    let emailNotification = EmailNotification.initialize getAllOffices
    
    let apiDependencyFactory = ApiDependencyFactory.provide eventStore reservationCommandsFactory getAllOffices emailNotification.SendEmailNotification

    let routes = Routes.provide apiDependencyFactory
    app.UseCors(configureCors ctx)
       .UseGiraffe routes.HttpHandlers
           
let configureAppConfiguration (builder : IConfigurationBuilder) =
    let region = Environment.GetEnvironmentVariable("AWS_REGION")
    let environment = Environment.GetEnvironmentVariable("ENVIRONMENT")
    let awsKeyId = Environment.GetEnvironmentVariable("AWS_KEYID")
    let awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRETKEY")
    let credentials = BasicAWSCredentials(awsKeyId, awsSecretKey)
    let mutable options = AWSOptions()
    options.Region <- RegionEndpoint.GetBySystemName(region)
    options.Credentials <- credentials
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
            .AddCors()
            .AddDefaultAWSOptions(config.GetAWSOptions())
            .AddAWSService<IAmazonDynamoDB>() |> ignore
    configureDynamoDB serviceProvider

let configureEmailService (sp : ServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    let emailServiceConfiguration =
        {
            SmtpClientUrl = config.["SMTP:ClientUrl"]
            SmtpUsername = config.["SMTP:Username"]
            SmtpPassword = config.["SMTP:Password"]
            EmailSender = config.["SMTP:EmailSender"]
            EmailReviewer = config.["SMTP:EmailReviewer"]
        } 
    Console.WriteLine(emailServiceConfiguration.SmtpClientUrl)
    Console.WriteLine(emailServiceConfiguration.SmtpUsername)

[<EntryPoint>]
let main _ =
    Host.CreateDefaultBuilder()
        .ConfigureWebHostDefaults(
            fun webHostBuilder ->
                webHostBuilder
                    .Configure(Action<_,_> configureApp)
                    .ConfigureAppConfiguration(Action<IConfigurationBuilder> configureAppConfiguration)
                    .ConfigureServices(configureServices)
                    |> ignore)
        .Build()
        .Run()
    0
