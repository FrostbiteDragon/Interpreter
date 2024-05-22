namespace FrostScript.Domain

type RailwayResult<'TSuccess, 'TFailure> =
    | Success of success: 'TSuccess
    | Failure of failure: 'TFailure
    | NotFound

module Railway =
    let (>=>) (switch1 : ParseHandler) (switch2 : ParseHandler) : ParseHandler = fun next ctx ->
        match switch1 next ctx with
        | Ok ctx -> 
            if ctx.Tokens.IsEmpty then Ok ctx
            else switch2 next ctx
        | Error m -> Error m