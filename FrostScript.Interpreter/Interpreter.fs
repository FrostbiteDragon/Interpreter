namespace FrostScript
open FrostScript.Core

module Interpreter =
    let interpret : Interpreter = fun nativeFunctions expressions ->

        let rec execute (ids : IdentifierMap<Expression>) (expression : Expression) : obj * IdentifierMap<Expression> =
            match expression.Type with
            | BinaryExpression (opporator, left, right) ->
                match expression.DataType with
                | NumberType -> 
                    let left = execute ids left |> fst
                    let right = execute ids right |> fst

                    let result = 
                        match opporator with
                        | Plus  -> box ((left :?> double) + (right :?> double)) 
                        | Minus -> box ((left :?> double) - (right :?> double))
                        | Slash -> box ((left :?> double) / (right :?> double))
                        | Star  -> box ((left :?> double) * (right :?> double))
                        | _ -> ()

                    (result, ids)

            | LiteralExpression value -> (value, ids)
            | IdentifierExpression id ->
               execute ids ids.[id]

            | AssignExpression (id, value) ->
                ((), ids.Change id value)

            | BindExpression (id, value) ->
                ((), ids.ChangeLocal id value)
                
            | ValidationError (token, message) ->
                printfn "(Line:%i Character:%i) %s" token.Line token.Character message
                ((), ids)

            | CallExpression (callee, argument) ->
                let (callee, ids) = execute ids callee
                let argument = execute ids argument |> fst
                match (callee :?> Expression).Type with
                | FrostFunction (closure, call) -> 
                    let result = call argument
                    (result, ids)
                | _ -> failwith "expression was not callable"

            | BlockExpression body ->
                let (results, ids) =
                    body
                    |> List.mapFold(fun ids expression -> execute ids expression) {globalIds = ids.Ids; localIds = Map.empty}
                (results |> List.last, {globalIds = Map.empty; localIds = ids.globalIds} )
                
            //| FunctionExpression (paramater, body) ->
            //    ({ DataType = body.DataType
            //       Type =
            //           FrostFunction (ids, fun argument ->
            //                let argumentExpression = 
            //                    { DataType = body.DataType
            //                      Type = (LiteralExpression (argument)) }
            //                execute (ids.Change paramater.Id argument) body) }
            //    , ids)

            | NativeFunction call -> ({ DataType = expression.DataType; Type = FrostFunction (ids, call) }, ids)
            | FrostFunction _ -> failwith "Do not use FrostFunction, use NativeFunction Instead"

        expressions
        |> List.mapFold(fun ids expression -> execute ids expression) { globalIds = Map.empty; localIds = Map nativeFunctions }
        |> fst
        |> List.last
