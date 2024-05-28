namespace FrostScript.Domain

module Railway =
    let (>=>) switch1 switch2 x =
        match switch1 x with
        | Ok s -> switch2 s
        | Error f -> Error f

    let rec choose funcList x =
        match funcList with
        | [] -> failwith "no option found"
        | func :: tail ->
            let result = func x
            match result with
            | Some s -> s
            | None   -> choose tail x