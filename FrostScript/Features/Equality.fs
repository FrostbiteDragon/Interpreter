[<AutoOpen>]
module FrostScript.Features.Equality
    open FrostScript.Domain

    let private operators = [("=", Equal); ("!=", NotEqual)]

    let private bind2 func result1 result2  =
        result1
        |> Result.bind (fun x -> result2 |> Result.bind (func x))

    let equality = {
        Lexer  = binaryLexer operators
        Parser = binaryParser operators

        Validator = fun validate ctx ->
            match ctx.Node.Type with
            | BinaryNode (operator, left, right) ->
                (validate left, validate right)
                ||> bind2 (fun left right ->
                    Ok { 
                        Expression = { DataType = BoolType; Type = BinaryExpression (operator, left.Expression, right.Expression)}
                        Ids = right.Ids
                    }
                )
                |> Some
                    
            | _ -> None
                    
        Interpreter = fun interpret ctx -> 
            match ctx.Expression.Type with
            | BinaryExpression (operator, leftExpression, rightExpression) ->
                if operators |> List.exists (fun (_, op) -> op = operator) then
                    interpret leftExpression
                    |> Result.bind (fun left -> 
                        interpret rightExpression
                        |> Result.map (fun right ->
                            match operator with
                            | Equal  -> box (left.Value = right.Value)
                            | NotEqual -> box (left.Value <> right.Value)
                            | _ -> ()
                        )
                    )
                    |> Result.map (fun value -> { Value = value; Ids = ctx.Ids })
                    |> Some
                else None
            | _ -> None
        }
