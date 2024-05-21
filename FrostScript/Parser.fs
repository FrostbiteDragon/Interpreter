namespace FrostScript
open Utilities

module Parser =
    let parse (tokens : Token list) =
        tokens
        |> splitTokens
        |> List.map (fun tokens -> ParserFunctions.expression tokens |> fst)
