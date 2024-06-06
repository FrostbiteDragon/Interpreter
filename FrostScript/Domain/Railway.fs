
[<AutoOpen>]
module FrostScript.Domain.Railway
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
