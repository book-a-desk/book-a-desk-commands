namespace Book_A_Desk.Api

open System
open System.IdentityModel.Tokens.Jwt
open System.Threading

open FSharp.Control.Tasks.V2.ContextInsensitive
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens

module rec JwtTokenValidator =
    let validateTokenPure
        (configuration: OpenIdConnectConfiguration)
        issuer
        audience
        (bearerToken : string)
        =
        let issuer = issuer + "/oauth2/default"
        let test = JwtSecurityTokenHandler()
        printfn $"Audience is: {audience}"
        let signingKeys = configuration.SigningKeys        
        let validationParameters = TokenValidationParameters()
        validationParameters.RequireExpirationTime <- false
        validationParameters.RequireSignedTokens <- false
        validationParameters.ValidateIssuer <- true
        validationParameters.ValidIssuer <- issuer
        validationParameters.ValidateAudience <- true
        validationParameters.ValidAudience <- audience
        validationParameters.ValidateIssuerSigningKey <- false
        validationParameters.ValidateLifetime <- false
        validationParameters.ClockSkew <- TimeSpan.FromMinutes(2.0)
        
        
        try
            let test = JwtSecurityTokenHandler()
            
            let jwtSecurityToken = test.ReadJwtToken(bearerToken)
            
            (*let _, token =
                (JwtSecurityTokenHandler())
                    .ValidateToken(bearerToken, validationParameters)
                            
            let jwtSecurityToken = (token :?> JwtSecurityToken)*)
            
            match manualValidation jwtSecurityToken issuer audience SecurityAlgorithms.RsaSha256 with
            | true ->
                    ValidToken
            | false ->
                    InvalidToken "Validation failed"
        with
        | e ->
            printfn $"Token Validation Error: {e}"
            InvalidToken $"Token Validation Error: {e}"
        
    let private manualValidation (jwtSecurityToken : JwtSecurityToken) issuer audience expectedAlg =
         printfn $"Expected issuer: {issuer}, Got: {jwtSecurityToken.Issuer}"
         printfn $"Expected audience: {audience}, Got: {jwtSecurityToken.Audiences |> List.ofSeq |> List.iter Console.WriteLine}"
         printfn $"Expected alg: {expectedAlg}, Got: {jwtSecurityToken.Header.Alg}"
         
         jwtSecurityToken.Issuer = issuer &&
         jwtSecurityToken.Audiences |> Seq.contains audience &&
         jwtSecurityToken.Header.Alg = expectedAlg
    
    
    let validateToken
        (configurationManager : IConfigurationManager<OpenIdConnectConfiguration>)
        issuer
        audience
        (bearerToken : string)
        = task {
        let issuer = issuer + "/oauth2/default"
        let test = JwtSecurityTokenHandler()
        printfn $"Audience is: {audience}"
        let! config = configurationManager.GetConfigurationAsync(CancellationToken.None)
        let signingKeys = config.SigningKeys        
        let validationParameters = TokenValidationParameters()
        validationParameters.RequireExpirationTime <- false
        validationParameters.RequireSignedTokens <- false
        validationParameters.ValidateIssuer <- true
        validationParameters.ValidIssuer <- issuer
        validationParameters.ValidateAudience <- true
        validationParameters.ValidAudience <- audience
        validationParameters.ValidateIssuerSigningKey <- false
        validationParameters.ValidateLifetime <- false
        validationParameters.ClockSkew <- TimeSpan.FromMinutes(2.0)
        
        
        try
            let test = JwtSecurityTokenHandler()
            
            let jwtSecurityToken = test.ReadJwtToken(bearerToken)
            
            (*let _, token =
                (JwtSecurityTokenHandler())
                    .ValidateToken(bearerToken, validationParameters)
                            
            let jwtSecurityToken = (token :?> JwtSecurityToken)*)
            
            match manualValidation jwtSecurityToken issuer audience SecurityAlgorithms.RsaSha256 with
            | true ->
                    return ValidToken
            | false ->
                    return InvalidToken "Validation failed"
        with
        | e ->
            printfn $"Token Validation Error: {e}"
            return InvalidToken $"Token Validation Error: {e}"
    }