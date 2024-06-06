
[<AutoOpen>]
module FrostScript.Domain.Railway
    let (>=>) switch1 switch2 x =
        match switch1 x with
        | Ok s -> switch2 s
        | Error f -> Error f

    let rec choose x funcList =
        match funcList with
        | [] -> failwith $"no option found for '{x}'"
        | func :: tail ->
            let result = func x
            match result with
            | Some s -> s
            | None   -> choose x tail

    let bindTraverse f = f |> List.traverseResult |> Result.bind

    let apply fOpt xOpt =
        match fOpt, xOpt with
        | Ok f, Ok x -> Ok (f x)
        | _ -> Error []
