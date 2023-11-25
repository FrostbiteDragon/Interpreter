namespace FrostScript
open Utilities

module Parser =
    let parse (tokens : Token list) =
        tokens
        |> splitTokens
        |> List.map (fun tokens ->
            let (node, _) = ParserFunctions.expression tokens
            node
        )