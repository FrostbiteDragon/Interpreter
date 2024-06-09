[<AutoOpen>]
module FrostScript.Features.Term
    open FrostScript.Domain

    let private operators = [("*", Multiply); ("/", Devide)]

    let term = {
        Lexer  = binaryLexer operators
        Parser = binaryParser operators

        Validator = fun validate ctx ->
            match ctx.Node.Type with
            | BinaryNode (operator, left, right) ->
                validate left
                |> Result.bind (fun left -> 
                    validate right
                    |> Result.bind (fun right ->
                        if left.Expression.DataType = NumberType && right.Expression.DataType = NumberType then
                            Ok { 
                                Expression = 
                                    { DataType = NumberType; Type = BinaryExpression (operator, left.Expression, right.Expression)}

                                Ids = right.Ids
                            }
                        else Error [(ctx.Node.Token, $"Opperator '{operator}' cannot be used between types {left.Expression.DataType} and {right.Expression.DataType}")]
                    )
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
                            | Multiply  -> box ((left.Value :?> double) * (right.Value :?> double))
                            | Devide -> box ((left.Value :?> double) / (right.Value :?> double))
                            | _ -> ()
                        )
                    )
                    |> Result.map (fun value -> { Value = value; Ids = ctx.Ids })
                    |> Some
                else None
            | _ -> None
        }
