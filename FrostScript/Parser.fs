module FrostScript.Parser
    open FrostScript.Domain
    open FrostScript.Domain.Utilities
    open FrostScript.Features
    open FrostScript.Domain.Railway

    let parse (tokens : Token list) =
        let rec expression : ParseFunc = fun ctx ->
            let ifNotEmpty onNotEmpty : ParseFunc = fun ctx -> if ctx.Tokens.IsEmpty then Ok ctx else onNotEmpty ctx

            FrostList.parse expression >=>
            ifNotEmpty Literal.parse 
            <| ctx

        tokens
        |> splitTokens
        |> List.map (fun tokens -> expression { Node = { Token = tokens.Head; Type = StatementNode}; Tokens = tokens })
        //flip result from list<result> to Result<list>