open System
open Amazon
open Amazon.DynamoDBv2
open Amazon.Extensions.NETCore.Setup
open Amazon.Runtime
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection
open Giraffe

open Book_A_Desk.Api
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Infrastructure
open Book_A_Desk.Domain.CommandHandler
 
let useDevelopmentStorage = Environment.GetEnvironmentVariable("AWS_DEVELOPMENTSTORAGE") |> bool.Parse

let configureCors (ctx : WebHostBuilderContext) (builder : CorsPolicyBuilder) =
    if ctx.HostingEnvironment.IsDevelopment() || useDevelopmentStorage then
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
    let provideEventStore amazonDynamoDb = DynamoDbEventStore.provide amazonDynamoDb

    let getAllOffices = (fun () -> Offices.All)

    let validDomainName = ctx.Configuration.["DomainName"]
    let reservationCommandsFactory = ReservationCommandsFactory.provide getAllOffices validDomainName
    
    let smtpClientManager = SmtpClientManager.provide
    let getEmailServiceConfiguration = (fun () -> app.ApplicationServices.GetService<EmailServiceConfiguration>())
    let getFeatureFlagsServiceConfiguration = (fun () -> app.ApplicationServices.GetService<FeatureFlags>())
    let bookingNotifier = BookingNotifier.provide getEmailServiceConfiguration smtpClientManager.SmtpClient getAllOffices
    
    let apiDependencyFactory = ApiDependencyFactory.provide provideEventStore reservationCommandsFactory getAllOffices bookingNotifier.NotifySuccess getFeatureFlagsServiceConfiguration

    let routes = Routes.provide apiDependencyFactory
    app.UseCors(configureCors ctx)
       .UseGiraffe routes.HttpHandlers
           
let configureAppConfiguration (builder : IConfigurationBuilder) =
    match useDevelopmentStorage with
    | false ->
        let region = Environment.GetEnvironmentVariable("AWS_REGION")
        let awsKeyId = Environment.GetEnvironmentVariable("AWS_KEYID")
        let awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRETKEY")
        let credentials = BasicAWSCredentials(awsKeyId, awsSecretKey)
        let options = AWSOptions()
        options.Region <- RegionEndpoint.GetBySystemName(region)
        options.Credentials <- credentials
        builder.AddSystemsManager($"/BookADesk/", options) |> ignore
    | true -> ()
    
let configureDynamoDB (sp : ServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    let dynamoDBConfiguration =
        {
            ReservationTableName = config.["DynamoDB:ReservationTableName"]
            OfficeTableName = config.["DynamoDB:OfficeTableName"]
        }
    printfn $"ReservationTableName: {dynamoDBConfiguration.ReservationTableName}"
    printfn $"OfficeTableName: {dynamoDBConfiguration.OfficeTableName}"

let configureFeatureFlags (sp : ServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    {
        BookingCancellation = config.["FeatureFlags:BookingCancellation"] |> bool.Parse
    }
        
let configureEmailService (sp : ServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    {
        SmtpClientUrl = config.["SMTP:ClientUrl"]
        SmtpClientPort = config.["SMTP:ClientPort"] |> int
        SmtpUsername = config.["SMTP:Username"]
        SmtpPassword = config.["SMTP:Password"]
        EmailSender = config.["SMTP:EmailSender"]
        EmailReviewer = config.["SMTP:EmailReviewer"]
    }

let configureServices (services : IServiceCollection) =
    let serviceProvider = services.BuildServiceProvider()
    let config = serviceProvider.GetService<IConfiguration>()
    
    services.AddGiraffe()
            .AddSingleton<FeatureFlags>(configureFeatureFlags serviceProvider)
            .AddCors() |> ignore
            
    match useDevelopmentStorage with
    | false ->
        services
            .AddDefaultAWSOptions(config.GetAWSOptions())
            .AddSingleton<EmailServiceConfiguration>(configureEmailService serviceProvider)
            .AddAWSService<IAmazonDynamoDB>() |> ignore
    | true ->
        services.AddSingleton<IAmazonDynamoDB>(fun _ ->
            let localAmazonDynamoDB = Environment.GetEnvironmentVariable("AWS_DEVELOPMENTURL")
            let clientConfig = AmazonDynamoDBConfig()
            clientConfig.ServiceURL <- localAmazonDynamoDB
            new AmazonDynamoDBClient(clientConfig) :> IAmazonDynamoDB
        ) |> ignore
        
    configureDynamoDB serviceProvider

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
