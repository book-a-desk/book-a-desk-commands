module Book_A_Desk.Api.Tests.HttpRequest

open System.Net.Http
open System.Text

let getAsyncGetContent (httpClient : HttpClient) (url : string) =
    async {
        let! response = httpClient.GetAsync(url) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }
    
let postAsyncGetContent (httpClient : HttpClient) (url : string) content =
    async {
        let content = new StringContent(content, Encoding.UTF8, "application/json")
        let! response = httpClient.PostAsync(url, content) |> Async.AwaitTask
        response.EnsureSuccessStatusCode () |> ignore
        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        return content
    }

let sendPostAsyncRequest (httpClient : HttpClient) (url : string) (content: string) =
    async {
        let postRequest = new HttpRequestMessage(HttpMethod.Post, url)
        let content = new StringContent(content, Encoding.UTF8, "application/json")
        postRequest.Content <- content

        return httpClient.SendAsync postRequest |> Async.AwaitTask |> Async.RunSynchronously
    }