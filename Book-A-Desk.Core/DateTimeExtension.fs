namespace Book_A_Desk.Core

open System

module DateTime =
    let TryParseOption (date : string) =
        match DateTime.TryParse date with
        | true, date -> Some date
        | false, _ -> None