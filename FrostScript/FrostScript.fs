module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute =
        let lex (script : string) : Result<Token list, (Token * string) list> = 
            let ctx = { Characters = script.ToCharArray () |> Array.toList; Position = { Character = 0; Line = 0; }; Tokens = [] }

            ctx 
            |> choose [ Literal.lex ]
            |> Result.map (fun x -> x.Tokens)

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

            ctx 
            |> choose [ Literal.validate ]

        let interpret (expression : Expression) : Result<obj, (Token * string) list> =
            expression
            |> choose [ Literal.interpret ]

        lex >> 
        apply (Ok splitTokens) >> 
        bindTraverse parse >> 
        bindTraverse validate >> 
        bindTraverse interpret >> 
        Result.map (fun x -> x |> List.last)