[<AutoOpen>]
module FrostScript.Domain.Utilities
    let valueOrUnit (option : obj option) =
            match option with
            | Some value -> value
            | None -> ()

    let skipOrEmpty count list =
        if list |> List.isEmpty then []
        else if count > list.Length then []
        else list |> List.skip count
