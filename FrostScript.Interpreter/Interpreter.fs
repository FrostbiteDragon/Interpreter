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

            | LiteralExpression value -> value
            | IdentifierExpression id ->
               identifiers.[id] |> execute

            | AssignExpression (id, value) ->
                identifiers <- identifiers.Change(id, fun _ -> Some value)
                ()

            | BindExpression (id, value) ->
                identifiers <- identifiers.Change(id, fun _ -> Some value)
                ()

            | ValidationError (message) -> 
                printfn "(Line:%i Character:%i) %s" expression.Token.Line expression.Token.Character message
                ()

        expressions
        |> List.map(fun x -> execute x)
        |> List.last
