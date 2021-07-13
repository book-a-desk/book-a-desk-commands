namespace Book_A_Desk.Api.Tests

open Microsoft.Extensions.Configuration

// This mock is required because we fetch it through the dependency injection system to create the EmailServiceConfiguration

type MockEmailServiceConfiguration () =
    interface IConfiguration with
        member this.get_Item(key:string) =
            match key with
            | "SMTP:ClientUrl" -> "http://localhost:8080/SMTP"
            | "SMTP:Username" -> "username"
            | "SMTP:Password" -> "password"
            | "SMTP:EmailSender" -> "from@broadsing.com"
            | "SMTP:EmailReviewer" -> "reviewer@broadsing.com"
            | _ -> ""
            
        member this.set_Item(key:string, value:string) = ()    
            
        member this.GetSection(key:string) = failwith "todo"
        member this.GetChildren() = failwith "todo"
        member this.GetReloadToken() = failwith "todo"                