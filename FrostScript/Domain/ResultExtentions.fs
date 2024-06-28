module Result

let bind2 func result1 result2  =
    result1
    |> Result.bind (fun x -> result2 |> Result.bind (func x))

