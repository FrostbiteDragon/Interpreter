module FrostScript.Parser
    open FrostScript.Domain
    open FrostScript.Domain.Utilities

    let parse (tokens : Token list) =
        tokens
        |> splitTokens
        |> List.map (fun tokens -> ParserFunctions.expression { Node = { Token = tokens.Head; Type = StatementNode}; Tokens = tokens })
