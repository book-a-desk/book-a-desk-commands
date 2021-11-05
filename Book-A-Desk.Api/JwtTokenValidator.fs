namespace Book_A_Desk.Api

open System
open System.IdentityModel.Tokens.Jwt
open System.Threading

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens

module JwtTokenValidator =
    let validateToken
        (configurationManager : IConfigurationManager<OpenIdConnectConfiguration>)
        issuer
        audience
        (bearerToken : string)
        = task {
            
        let! config = configurationManager.GetConfigurationAsync(CancellationToken.None)
        let signingKeys = config.SigningKeys        
        let validationParameters = TokenValidationParameters()
        validationParameters.RequireExpirationTime <- true
        validationParameters.RequireSignedTokens <- true
        validationParameters.ValidateIssuer <- true
        validationParameters.ValidIssuer <- issuer
        validationParameters.ValidateAudience <- true
        validationParameters.ValidAudience <- audience
        validationParameters.ValidateIssuerSigningKey <- true
        validationParameters.IssuerSigningKeys <- signingKeys
        validationParameters.ValidateLifetime <- true
        validationParameters.ClockSkew <- TimeSpan.FromMinutes(2.0)
        
        try
            let _, token =
                (JwtSecurityTokenHandler())
                    .ValidateToken(bearerToken, validationParameters)
                            
            let jwtSecurityToken = (token :?> JwtSecurityToken)
            
            match jwtSecurityToken <> null with
            | true ->
                // Okta uses RsaSha256
                let expectedAlg = SecurityAlgorithms.RsaSha256
                let header = jwtSecurityToken.Header
                
                match header <> null && header.Alg = expectedAlg with
                | true ->
                    return ValidToken
                | false -> return InvalidToken "Unexpected algorithm"
            | false -> return InvalidToken "Did not get a security token"
        with
        | e ->
            return InvalidToken $"Token Validation Error: {e}"
    }