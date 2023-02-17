module Book_A_Desk.Api.Tests.JwtTokenValidatorTest

open System.IO
open System.Threading
open Book_A_Desk.Api
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Newtonsoft.Json
open Xunit

type TestFile =
    {
        BearerToken : string
    }


[<Fact>]
let ``Given a valid bearer token When validating the token Then the bearer token is valid`` () = async {
    let oktaIssuer = "https://dev-05054243.okta.com/oauth2/default"
    let metadataAddress = oktaIssuer + "/.well-known/openid-configuration"
    let audience = "0oa3x87srayaxvqxS5d7"
    let configurationManager = ConfigurationManager<OpenIdConnectConfiguration>(
        metadataAddress,
        OpenIdConnectConfigurationRetriever())
    let! configuration = configurationManager.GetConfigurationAsync(CancellationToken.None) |> Async.AwaitTask
    
    let testFileContent = File.ReadAllText "testConfig.json"
    let testFile = JsonConvert.DeserializeObject<TestFile> testFileContent
    
    let bearerToken = testFile.BearerToken
    
    let validatedToken =
        JwtTokenValidator.validateTokenPure
            configuration
            oktaIssuer
            audience
            bearerToken
        
    match validatedToken with
    | ValidToken -> ()
    | InvalidToken error -> failwith $"Token was invalid: {error}"
}