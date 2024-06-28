[<AutoOpen>]
module FrostScript.Features.Equality
    open FrostScript.Domain

    let private operators = [("=", Equal); ("!=", NotEqual)]

    let equality = {
        Lexer  = binaryLexer operators
        Parser = binaryParser operators

        Validator = binaryValidator (fun operator _ left right ->
            Ok { 
                Expression = { DataType = BoolType; Type = BinaryExpression (operator, left.Expression, right.Expression)}
                Ids = right.Ids
            }
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
