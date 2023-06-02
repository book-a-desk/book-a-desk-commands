namespace Book_A_Desk.Api

open System
open System.IdentityModel.Tokens.Jwt

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens

module rec JwtTokenValidator =
    let validateToken
        metadataAddress
        (configuration: ConfigurationManager<OpenIdConnectConfiguration>)
        audience
        (bearerToken : string)
        = task {
            try
                let! config = configuration.GetConfigurationAsync()
                return validateTokenWithConfig config audience bearerToken 
            with
            | e ->
                return ConnectionError $"Connection Error while getting configuration for {metadataAddress}: {e}"
        }
            
    let validateTokenWithConfig 
        (configuration: OpenIdConnectConfiguration)
        audience
        (bearerToken : string)
        =
        let signingKeys = configuration.SigningKeys        
        let validationParameters = TokenValidationParameters()
        validationParameters.RequireExpirationTime <- false
        validationParameters.RequireSignedTokens <- true
        validationParameters.ValidateIssuer <- true
        validationParameters.ValidIssuer <- configuration.Issuer
        validationParameters.ValidateAudience <- true
        validationParameters.ValidAudience <- audience
        validationParameters.ValidateIssuerSigningKey <- true
        validationParameters.ValidateLifetime <- false
        validationParameters.ClockSkew <- TimeSpan.FromMinutes(2.0)
        validationParameters.ValidAlgorithms <- [SecurityAlgorithms.RsaSha256]
        validationParameters.IssuerSigningKeys <- signingKeys
        
        try
            JwtSecurityTokenHandler()
                .ValidateToken(bearerToken, validationParameters)
                |> ignore
                            
            ValidToken
        with
        | e ->
            printfn $"Token Validation Error: {e}"
            InvalidToken $"Token Validation Error: {e}"