open System
open System.Threading.Tasks
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
open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain
open Book_A_Desk.Infrastructure
open Book_A_Desk.Domain.CommandHandler
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open Okta.AspNetCore
 
let useDevelopmentStorage = Environment.GetEnvironmentVariable("AWS_DEVELOPMENTSTORAGE") |> bool.Parse

let getOktaIssuer oktaDomain = $"https://{oktaDomain}/oauth2/default"

let getConfigurationManager oktaIssuer =
    ConfigurationManager<OpenIdConnectConfiguration>(
        oktaIssuer + "/.well-known/oauth-authorization-server",
        OpenIdConnectConfigurationRetriever(),
        HttpDocumentRetriever())
    

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

let configureFeatureFlags (config : IConfiguration) =
    {
        BookingCancellation = config.["FeatureFlags:BookingCancellation"] |> bool.Parse
        GetBookings = config.["FeatureFlags:GetBookings"] |> bool.Parse
    }

let configureApp (ctx : WebHostBuilderContext) (app : IApplicationBuilder) =
    let provideEventStore amazonDynamoDb = DynamoDbEventStore.provide amazonDynamoDb

    let getAllOffices = (fun () -> Offices.All)

    let validDomainName = ctx.Configuration.["DomainName"]
    let reservationCommandsFactory = ReservationCommandsFactory.provide getAllOffices validDomainName
    
    let smtpClientManager = SmtpClientManager.provide
    let getEmailServiceConfiguration = (fun () -> app.ApplicationServices.GetService<EmailServiceConfiguration>())
    let bookingNotifier = BookingNotifier.provide getEmailServiceConfiguration smtpClientManager.SmtpClient getAllOffices
    let officeRestrictionNotifier = OfficeRestrictionNotifier.provide bookingNotifier.NotifyOfficeRestrictionToBooking

    let featureFlags = configureFeatureFlags ctx.Configuration
    
    let apiDependencyFactory = ApiDependencyFactory.provide
                                   provideEventStore
                                   reservationCommandsFactory
                                   getAllOffices
                                   bookingNotifier.NotifySuccess
                                   officeRestrictionNotifier.NotifyOfficeRestrictions
                                   featureFlags

    // TODO: Fix Okta issue in the following Mob programming sessions
    // let oktaDomain = ctx.Configuration.["Okta:OktaDomain"]
    // let oktaIssuer = getOktaIssuer oktaDomain
    // let configurationManager = getConfigurationManager oktaIssuer
    // let oktaAudience = ctx.Configuration.["Okta:OktaAudience"]
    //
    // let validateToken = JwtTokenValidator.validateToken configurationManager oktaIssuer oktaAudience
    
    let routes = Routes.provide apiDependencyFactory (fun _ -> Task.FromResult ValidToken)
    // TODO: Fix Okta issue in the following Mob programming sessions
    // app.UseAuthentication()
    //    .UseAuthorization() |> ignore
    
    app.UseCors(configureCors ctx)
       .UseGiraffe routes.HttpHandlers
           
let configureAppConfiguration (builder : IConfigurationBuilder) =
    match useDevelopmentStorage with
    | false ->
        builder.AddSystemsManager($"/BookADesk/", AWSOptions()) |> ignore
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
    
    // TODO: Fix Okta issue in the following Mob programming sessions
    // let oktaDomain = config.["Okta:OktaDomain"]
    // let oktaOptions = OktaWebApiOptions()
    // oktaOptions.OktaDomain <- oktaDomain
    
    services.AddGiraffe()
            .AddCors()
            // TODO: Fix Okta issue in the following Mob programming sessions
            // .AddAuthentication(
            //     fun options ->
            //         options.DefaultAuthenticateScheme <- OktaDefaults.ApiAuthenticationScheme
            //         options.DefaultChallengeScheme <- OktaDefaults.ApiAuthenticationScheme
            //         options.DefaultSignInScheme <- OktaDefaults.ApiAuthenticationScheme)
            // .AddOktaWebApi(oktaOptions)
            |> ignore
            
    services.AddAuthorization() |> ignore
            
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
