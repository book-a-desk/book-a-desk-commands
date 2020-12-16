namespace Book_A_Desk.Api

open Giraffe
open FSharp.Control.Tasks.V2.ContextInsensitive

open Book_A_Desk.Api.Models
open Book_A_Desk.Domain.Office.Domain

type OfficesHttpHandler =
    {
        HandleGet: unit -> HttpHandler
    }
    
module OfficesHttpHandler =
    let initialize () =
        let handleGet () = fun next context ->
            task {
                let offices =
                    Offices.All
                    |> List.map (fun o ->
                        let (OfficeId officeId) = o.Id
                        let (CityName cityName) = o.City
                        {
                            Id = officeId.ToString()
                            Name = cityName
                        })
                return! json offices next context
            }
        {
            HandleGet = handleGet
        }