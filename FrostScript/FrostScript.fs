module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute rawScript =
        let parse (input : Result<Token list list, (Token * string) list>) : Result<Node list, (Token * string) list> =
            let rec expression : ParseFunc = fun ctx ->
                let ifNotEmpty onNotEmpty : ParseFunc = fun ctx -> if ctx.Tokens.IsEmpty then Ok ctx else onNotEmpty ctx

                FrostList.parse expression >=>
                ifNotEmpty Literal.parse 
                <| ctx

            input
            |> Result.bind (fun tokenGroups -> tokenGroups |> List.traverseResult (fun tokens ->
                let ctx = { Tokens = tokens; Node = { Token = tokens.Head; Type = StatementNode } }
            
                expression ctx
                |> Result.map (fun ctx -> ctx.Node)
            ))

        let validate (input : Result<Node list, (Token * string) list>) : Result<Expression list, (Token * string) list> =
            input
            |> Result.bind (fun nodes -> nodes |> List.traverseResult (fun node -> 
                let ctx = { Node = node; Ids = { Values = [] } }
                choose [Literal.validate] ctx
            ))
        
        parse >> validate
