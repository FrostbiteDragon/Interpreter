namespace FrostScript
open FrostScript.Core

module Interpreter =
    let interpret : Interpreter = fun nativeFunctions expressions ->

        let rec execute (ids : IdentifierMap<Expression>) (expression : Expression) : obj * IdentifierMap<Expression> =
            match expression.Type with
            | BinaryExpression (opporator, left, right) ->
                let left = execute ids left |> fst
                let right = execute ids right |> fst

                let result =
                    match expression.DataType with
                    | NumberType -> 
                        match opporator with
                        | Plus  -> box ((left :?> double) + (right :?> double)) 
                        | Minus -> box ((left :?> double) - (right :?> double))
                        | Slash -> box ((left :?> double) / (right :?> double))
                        | Star  -> box ((left :?> double) * (right :?> double))
                        | GreaterThen     -> box ((left :?> double) > (right :?> double))
                        | GreaterOrEqual  -> box ((left :?> double) >= (right :?> double))
                        | LessThen        -> box ((left :?> double) < (right :?> double))
                        | LessOrEqual     -> box ((left :?> double) <= (right :?> double))
                        | NotEqual -> box ((left :?> double) <> (right :?> double))
                        | Equal    -> (left :?> double) = (right :?> double)
                        | _ -> ()

                    | BoolType ->
                        match opporator with
                        | NotEqual -> box ((left :?> bool) <> (right :?> bool))
                        | Equal    -> (left :?> bool) = (right :?> bool)
                        | _ -> ()

                    | StringType ->
                        match opporator with
                        | Plus -> box ((left :?> string) + (right :?> string))
                        | Equal -> (left :?> string) = (right :?> string)
                        | _ -> ()

                    | AnyType ->
                        match opporator with
                        | Equal -> left = right
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
                let (argument, ids) = execute ids argument
                match (callee :?> Expression).Type with
                | FrostFunction (call) -> 
                    call ids argument
                | _ -> failwith "expression was not callable"

            | BlockExpression body ->
                let (results, ids) =
                    body
                    |> List.mapFold(fun ids expression -> execute ids expression) {globalIds = ids.Ids; localIds = Map.empty}
                (results |> List.last, {globalIds = Map.empty; localIds = ids.globalIds} )
                
            | FunctionExpression (paramater, body) ->
                ({ DataType = body.DataType
                   Type =
                       FrostFunction (fun ids argument ->
                            let argumentExpression = 
                                { DataType = paramater.Value
                                  Type = (LiteralExpression (argument)) }
                            execute (ids.Change paramater.Id argumentExpression) body) }
                , ids)

            | NativeFunction call -> ({ DataType = expression.DataType; Type = FrostFunction (fun ids argument -> (call argument, ids)) }, ids)
            | FrostFunction _ -> failwith "Do not use FrostFunction, use NativeFunction Instead"

        expressions
        |> List.mapFold(fun ids expression -> execute ids expression) { globalIds = Map.empty; localIds = Map nativeFunctions }
        |> fst
        |> List.last
