module Book_A_Desk.Api.Tests.HttpRequest

open System.Net.Http
open System.Text

let getAsync (httpClient : HttpClient) (url : string) =        
    async {
        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }
    
let postAsync (httpClient : HttpClient) (url : string) content =
    async {
        let content = new StringContent(content, Encoding.UTF8, "application/json")
        let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

