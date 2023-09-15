namespace FrostScript
open FrostScript.Core

module Interpreter =
    let interpret : Interpreter = fun expressions ->
        let rec execute (expression : Expression) : obj =
            match expression with
            | Expression.Binary (token, dataType, left, right) -> 
                let left = execute left :?> int
                let right = execute right :?> int

                match token.Type with
                | Plus -> box (left + right) 
                | Minus -> box (left - right)
                | _ -> ()

            | Expression.Primary (token, _) -> 
                match token.Literal with
                | Some value -> value
                | _ -> ()

        expressions
        |> List.map(fun x -> execute x)
        |> List.last
