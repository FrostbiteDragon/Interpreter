namespace FrostScript.Domain

type RailwayResult<'TSuccess, 'TFailure> =
    | Success of success: 'TSuccess
    | Failure of failure: 'TFailure
    | NotFound

module Railway =
    let (>=>) (switch1 : ParserHandler) (switch2 : ParserHandler) : ParserHandler = fun next ctx ->
        match switch1 next ctx with
        | Ok ctx -> switch2 next ctx
        | Error m -> Error m