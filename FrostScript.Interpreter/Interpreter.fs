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

            | CallExpression (callee, argument) ->
                let callee = execute callee :?> ExpressionType
                let argument = execute argument
                match callee with
                | FrostFunction (closure, call) -> 
                    let currentIdentifiers = identifiers
                    identifiers <- closure
                    let result = call argument
                    identifiers <- currentIdentifiers
                    result
                | _ -> failwith "expression was not callable"
                
            | FunctionExpression (paramater, body) ->
                FrostFunction (identifiers, fun argument ->
                    let argumentExpression = 
                        { Token = body.Token
                          DataType = body.DataType
                          Type = (LiteralExpression (argument)) }
                    identifiers <- identifiers.Change(paramater.Id, fun _ -> Some argumentExpression)
                    execute(body))

            | NativeFunction call -> FrostFunction (identifiers, call)
        expressions
        |> List.map(fun x -> execute x)
        |> List.last
