namespace FrostScript
open FrostScript.Core

module Interpreter =
    let interpret : Interpreter = fun nativeFunctions expressions ->
        let mutable identifiers = Map nativeFunctions

        let rec execute (expression : Expression) : obj =
            match expression.Type with
            | BinaryExpression (left, right) ->
                match expression.DataType with
                | NumberType -> 
                    let left = execute left :?> double
                    let right = execute right :?> double

                    match expression.Token.Type with
                    | Plus  -> box (left + right) 
                    | Minus -> box (left - right)
                    | Slash -> box (left / right)
                    | Star  -> box (left * right)
                    | _ -> ()

            | LiteralExpression (value) -> value
            | IdentifierExpression ->
               identifiers.[expression.Token.Lexeme] |> execute

            | BindExpression (value) ->
                identifiers <- identifiers.Change(expression.Token.Lexeme, fun _ -> Some value)
                ()

        expressions
        |> List.map(fun x -> execute x)
        |> List.last
