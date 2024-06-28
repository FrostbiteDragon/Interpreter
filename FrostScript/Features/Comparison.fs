[<AutoOpen>]
module FrostScript.Features.Equality
    open FrostScript.Domain

    let private operators = [(">", GreaterThen); ("<", LessThen); (">=", GreaterOrEqual); ("<=", LessOrEqual)]

    let equality = {
        Lexer  = binaryLexer operators
        Parser = binaryParser operators

        Validator = binaryValidator (fun operator ctx left right ->
            if left.Expression.DataType = NumberType && right.Expression.DataType = NumberType then
                Ok { 
                    Expression = { DataType = BoolType; Type = BinaryExpression (operator, left.Expression, right.Expression)}
                    Ids = right.Ids
                }
            else Error [(ctx.Node.Token, $"Operator '{operator}' cannot be used between types {left.Expression.DataType} and {right.Expression.DataType}")]
        )
                    
        Interpreter = fun interpret ctx -> 
            match ctx.Expression.Type with
            | BinaryExpression (operator, leftExpression, rightExpression) ->
                if operators |> List.exists (fun (_, op) -> op = operator) then
                    interpret leftExpression
                    |> Result.bind (fun left -> 
                        interpret rightExpression
                        |> Result.map (fun right ->
                            match operator with
                            | GreaterThen  -> box ((left.Value :?> double) > (right.Value :?> double))
                            | GreaterOrEqual -> box ((left.Value :?> double) >= (right.Value :?> double))
                            | LessThen -> box ((left.Value :?> double) < (right.Value :?> double))
                            | LessOrEqual -> box ((left.Value :?> double) <= (right.Value :?> double))
                            | _ -> ()
                        )
                    )
                    |> Result.map (fun value -> { Value = value; Ids = ctx.Ids })
                    |> Some
                else None
            | _ -> None
        }
