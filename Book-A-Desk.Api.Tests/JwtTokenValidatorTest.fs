module Book_A_Desk.Api.Tests.JwtTokenValidatorTest

open System.Collections.Generic
open System.IO
open System.Security.Cryptography
open System.Text
open System.Text.Unicode
open System.Threading
open Book_A_Desk.Api
open Microsoft.IdentityModel.Protocols
open Microsoft.IdentityModel.Protocols.OpenIdConnect
open Microsoft.IdentityModel.Tokens
open Newtonsoft.Json
open Org.BouncyCastle.Math.EC
open Xunit

type TestFile =
    {
        BearerToken : string
    }
    
let configurationJson ="""
    {
	"issuer": "https://dev-05054243.okta.com/oauth2/default",
	"authorization_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/authorize",
	"token_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/token",
	"userinfo_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/userinfo",
	"registration_endpoint": "https://dev-05054243.okta.com/oauth2/v1/clients",
	"jwks_uri": "https://dev-05054243.okta.com/oauth2/default/v1/keys",
	"response_types_supported": [
		"code",
		"id_token",
		"code id_token",
		"code token",
		"id_token token",
		"code id_token token"
	],
	"response_modes_supported": [
		"query",
		"fragment",
		"form_post",
		"okta_post_message"
	],
	"grant_types_supported": [
		"authorization_code",
		"implicit",
		"refresh_token",
		"password",
		"urn:ietf:params:oauth:grant-type:device_code"
	],
	"subject_types_supported": [
		"public"
	],
	"id_token_signing_alg_values_supported": [
		"RS256"
	],
	"scopes_supported": [
		"openid",
		"profile",
		"email",
		"address",
		"phone",
		"offline_access",
		"device_sso"
	],
	"token_endpoint_auth_methods_supported": [
		"client_secret_basic",
		"client_secret_post",
		"client_secret_jwt",
		"private_key_jwt",
		"none"
	],
	"claims_supported": [
		"iss",
		"ver",
		"sub",
		"aud",
		"iat",
		"exp",
		"jti",
		"auth_time",
		"amr",
		"idp",
		"nonce",
		"name",
		"nickname",
		"preferred_username",
		"given_name",
		"middle_name",
		"family_name",
		"email",
		"email_verified",
		"profile",
		"zoneinfo",
		"locale",
		"address",
		"phone_number",
		"picture",
		"website",
		"gender",
		"birthdate",
		"updated_at",
		"at_hash",
		"c_hash"
	],
	"code_challenge_methods_supported": [
		"S256"
	],
	"introspection_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/introspect",
	"introspection_endpoint_auth_methods_supported": [
		"client_secret_basic",
		"client_secret_post",
		"client_secret_jwt",
		"private_key_jwt",
		"none"
	],
	"revocation_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/revoke",
	"revocation_endpoint_auth_methods_supported": [
		"client_secret_basic",
		"client_secret_post",
		"client_secret_jwt",
		"private_key_jwt",
		"none"
	],
	"end_session_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/logout",
	"request_parameter_supported": true,
	"request_object_signing_alg_values_supported": [
		"HS256",
		"HS384",
		"HS512",
		"RS256",
		"RS384",
		"RS512",
		"ES256",
		"ES384",
		"ES512"
	],
	"device_authorization_endpoint": "https://dev-05054243.okta.com/oauth2/default/v1/device/authorize",
    "signingKeys": [
		{
			"kty": "RSA",
			"alg": "RS256",
			"kid": "QhktjLwN6Kj9cdvt7i1k5-86peIf7LFiVhQ52qIlIAc",
			"use": "sig",
			"e": "AQAB",
			"n": "g1sz_19vPol-uNBR-mmRx_1RTi2obVTf0luVELe7UwOLHNAMkGJZHYWT-XKyff0qWt6Wb8g68aYd2Pn2qk-6zNPlkdu4CSxJ37FjUAyKLH4cq4-dyQxje-_ds_Zvrv4IOEnvA37XzZg4LL7kOhJfGEmL6xfD6PsYM4cCAMy9uY7NpcwZ_kCmNGEV9Q13zZ31z0xlCrOY7OEm9RLpNPzIDSnDxAGRDyNKbXwVwp-XXWS7nJ9kCha49Z5JnVENrENkh_BJroxrbLjhET5c847TAd_c8sK_40w_ZEWs2EKZfL5ZwxJw7pscd63SVT30NcCvlMJjvknhuXqZWU_BzDW5aw"
		},
		{
			"kty": "RSA",
			"alg": "RS256",
			"kid": "xqXzzfcJvA9Y5uFMkj5fvB-y4bLlrYvcWLzj3Q3TvrA",
			"use": "sig",
			"e": "AQAB",
			"n": "mgMnLwzFsJ8t-AjUkBPfcZ8AHcv3v1bFMRlNEYiwbf4pwSBNYrk-fqm152_jrLYFJHkpbU6ju1v5Lb1xL17v_yd6_vMnx-3oOu9BvrllTcJl7bUBZxmz5xJj33-Whd6lwUZAqvCAiUcdb3KAnBgAKhLRfcRjMdpvJPYD0Gzl-iPNbXOdzAURUWnGQ3he_0o4sUhq1dtw6ypZWPD9OzTBmh_ba6qwh3iShwtT6h5N94JpaXhDKsNpKKTAKKehF6PRqLZ8nUp-32wkK3LbZgIwCDYd_S3fbbNmn_m8mJaaVhvexXTEpdHRUg2iDj157XMIbHb9mutZ3ltMfx0PVJms0Q"
		}
	]
}
"""

let keys = """{
	"keys": [
		{
			"kty": "RSA",
			"alg": "RS256",
			"kid": "QhktjLwN6Kj9cdvt7i1k5-86peIf7LFiVhQ52qIlIAc",
			"use": "sig",
			"e": "AQAB",
			"n": "g1sz_19vPol-uNBR-mmRx_1RTi2obVTf0luVELe7UwOLHNAMkGJZHYWT-XKyff0qWt6Wb8g68aYd2Pn2qk-6zNPlkdu4CSxJ37FjUAyKLH4cq4-dyQxje-_ds_Zvrv4IOEnvA37XzZg4LL7kOhJfGEmL6xfD6PsYM4cCAMy9uY7NpcwZ_kCmNGEV9Q13zZ31z0xlCrOY7OEm9RLpNPzIDSnDxAGRDyNKbXwVwp-XXWS7nJ9kCha49Z5JnVENrENkh_BJroxrbLjhET5c847TAd_c8sK_40w_ZEWs2EKZfL5ZwxJw7pscd63SVT30NcCvlMJjvknhuXqZWU_BzDW5aw"
		},
		{
			"kty": "RSA",
			"alg": "RS256",
			"kid": "xqXzzfcJvA9Y5uFMkj5fvB-y4bLlrYvcWLzj3Q3TvrA",
			"use": "sig",
			"e": "AQAB",
			"n": "mgMnLwzFsJ8t-AjUkBPfcZ8AHcv3v1bFMRlNEYiwbf4pwSBNYrk-fqm152_jrLYFJHkpbU6ju1v5Lb1xL17v_yd6_vMnx-3oOu9BvrllTcJl7bUBZxmz5xJj33-Whd6lwUZAqvCAiUcdb3KAnBgAKhLRfcRjMdpvJPYD0Gzl-iPNbXOdzAURUWnGQ3he_0o4sUhq1dtw6ypZWPD9OzTBmh_ba6qwh3iShwtT6h5N94JpaXhDKsNpKKTAKKehF6PRqLZ8nUp-32wkK3LbZgIwCDYd_S3fbbNmn_m8mJaaVhvexXTEpdHRUg2iDj157XMIbHb9mutZ3ltMfx0PVJms0Q"
		}
	]
}"""

let bearerToken = "eyJraWQiOiJRaGt0akx3TjZLajljZHZ0N2kxazUtODZwZUlmN0xGaVZoUTUycUlsSUFjIiwiYWxnIjoiUlMyNTYifQ.eyJzdWIiOiIwMHUzeDZ2NW00cHN5REhlUjVkNyIsIm5hbWUiOiJBbGV4IEdhZ27DqSIsImVtYWlsIjoiYWxleC5nYWduZUBicm9hZHNpZ24uY29tIiwidmVyIjoxLCJpc3MiOiJodHRwczovL2Rldi0wNTA1NDI0My5va3RhLmNvbS9vYXV0aDIvZGVmYXVsdCIsImF1ZCI6IjBvYTN4ODdzcmF5YXh2cXhTNWQ3IiwiaWF0IjoxNjc2MDQxOTE3LCJleHAiOjE2NzYwNDU1MTcsImp0aSI6IklELnYxNGhmWXBtOW1Db3BPRG5fRVhMdjEwVHVlNnI0WFFoVlo5WERIc0lnMVUiLCJhbXIiOlsicHdkIl0sImlkcCI6IjBvYTN4NzE3NThwRDd6dE9UNWQ3Iiwibm9uY2UiOiJSMGpHTkxrdWg2THJqSzlwd1Jycms0Z1NlcWhUQjU3am9FdlJnOEpUVm5QaUNJcnVXZnFoS0pHWHJBZnhuU29uIiwicHJlZmVycmVkX3VzZXJuYW1lIjoiYWxleC5nYWduZUBicm9hZHNpZ24uY29tIiwiYXV0aF90aW1lIjoxNjc2MDM5OTg5LCJhdF9oYXNoIjoiRXhCQTZQQ0Z4Tmp5eDJLRi16REJvQSJ9.P2QJZ-9LIgpe5Th8oPG47_Ne1y1_t9EeNGKbDkdyR4RasjtTODogvlIFiu6xHOcFkFH9CeOEjKav6L3quQIVQqqQZxo_v-BjSGy4mmBR8sNXdMABnRMArPtSmFixob5ez1Y0uSSJabHEpVwvbDfvYsGJU5z5I_T1SbK5GHG6wUN_SwY3FchzULFk1i53zk73miwJ711yY7yQErILn_zu1ntv4Q3j8u42lyWlA7DQAEhmQY-RRyZRTW9nXdsEHHMPoSN7Ew8IhiBQDPa5OXWLsPkbieoupjHhOktezpGssxaZUl09XaOaL0TFvvyOWevc7LUSmxSWsSOODIKEUODc8Q"

[<Fact>]
let ``Given a valid bearer token When validating the token Then the bearer token is valid`` () = async {
    let oktaIssuer = "https://dev-05054243.okta.com/oauth2/default"
    let audience = "0oa3x87srayaxvqxS5d7"
    let configuration = OpenIdConnectConfiguration(configurationJson)
    
    let rsa1 = RSA.Create()
    rsa1.ImportRSAPublicKey(Encoding.UTF8.GetBytes("QhktjLwN6Kj9cdvt7i1k5-86peIf7LFiVhQ52qIlIAc")) |> ignore
    let rsa2 = RSA.Create()
    rsa2.ImportRSAPublicKey(Encoding.UTF8.GetBytes("xqXzzfcJvA9Y5uFMkj5fvB-y4bLlrYvcWLzj3Q3TvrA")) |> ignore
    
    configuration.SigningKeys.Add(RsaSecurityKey(rsa1))
    configuration.SigningKeys.Add(RsaSecurityKey(rsa2))
    
    let validatedToken =
        JwtTokenValidator.validateTokenWithConfig
            configuration
            oktaIssuer
            audience
            bearerToken
        
    match validatedToken with
    | ValidToken -> ()
    | InvalidToken error -> failwith $"Token was invalid: {error}"
}