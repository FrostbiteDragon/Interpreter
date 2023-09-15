namespace FrostScript
open FrostScript.Core

module Interpreter =
    let interpret : Interpreter = fun expressions ->
        let rec execute (expression : Expression) : obj =
            match expression with
            | Expression.BinaryExpression (token, dataType, left, right) ->
                match dataType with
                | NumberType -> 
                    let left = execute left :?> double
                    let right = execute right :?> double

                    match token.Type with
                    | Plus  -> box (left + right) 
                    | Minus -> box (left - right)
                    | Slash -> box (left / right)
                    | Star  -> box (left * right)
                    | _ -> ()

            | Expression.PrimaryExpression (token, _) -> 
                match token.Literal with
                | Some value -> value
                | _ -> ()

        expressions
        |> List.map(fun x -> execute x)
        |> List.last
