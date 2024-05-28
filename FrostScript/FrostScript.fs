module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute =
        let bindTraverse f input = Result.bind (fun x -> x |> List.traverseResult f) input

        let parse (input : Result<Token list list, (Token * string) list>) : Result<Node list, (Token * string) list> =
            let rec expression : ParseFunc = fun ctx ->
                let ifNotEmpty onNotEmpty : ParseFunc = fun ctx -> if ctx.Tokens.IsEmpty then Ok ctx else onNotEmpty ctx

                FrostList.parse expression >=>
                ifNotEmpty Literal.parse 
                <| ctx

            input
            |> bindTraverse (fun tokens ->
                let ctx = { Tokens = tokens; Node = { Token = tokens.Head; Type = StatementNode } }
            
                expression ctx
                |> Result.map (fun ctx -> ctx.Node)
            )

        let validate (input : Result<Node list, (Token * string) list>) : Result<Expression list, (Token * string) list> =
            input
            |> bindTraverse (fun node -> 
                let ctx = { Node = node; Ids = { Values = [] } }

                choose [Literal.validate] ctx
            )

        Lexer.lex2 >> parse >> validate >> Interpreter.interpret2
