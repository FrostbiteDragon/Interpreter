module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute =
        let bindTraverse f = f |> List.traverseResult |> Result.bind

        let lex (script : string) : Result<Token list list, (Token * string) list> = Ok []

        let parse (tokens : Token list) : Result<Node, (Token * string) list> =
            let rec expression : ParseFunc = fun ctx ->
                let ifNotEmpty onNotEmpty : ParseFunc = fun ctx -> if ctx.Tokens.IsEmpty then Ok ctx else onNotEmpty ctx

                FrostList.parse expression >=>
                ifNotEmpty Literal.parse 
                <| ctx
            let ctx = { Tokens = tokens; Node = { Token = tokens.Head; Type = StatementNode } }
            
            expression ctx
            |> Result.map (fun ctx -> ctx.Node)

        let validate (node : Node) : Result<Expression, (Token * string) list> =
            let ctx = { Node = node; Ids = { Values = [] } }

            choose [Literal.validate] ctx

        let interpret (expression : Expression) : Result<obj, (Token * string) list> = Ok []

        lex >> bindTraverse parse >> bindTraverse validate >> bindTraverse interpret
