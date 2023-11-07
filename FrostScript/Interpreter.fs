namespace FrostScript

module Interpreter =
    let interpret nativeFunctions expressions =
        let rec execute (ids : Expression IdMap) (expression : Expression) : obj * Expression IdMap =
            match expression.Type with
            | BinaryExpression (opporator, leftExpression, right) ->
                let left = execute ids leftExpression |> fst
                let right = execute ids right |> fst

                let result =
                    match leftExpression.DataType with
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
                        | NotEqual -> left.Equals right |> not |> box
                        | Equal    -> left.Equals right
                        | _ -> ()

                    | BoolType ->
                        match opporator with
                        | And      -> (left :?> bool) && (right :?> bool)
                        | Or       -> (left :?> bool) || (right :?> bool)
                        | NotEqual -> left.Equals right |> not |> box
                        | Equal    -> left.Equals right
                        | _ -> ()

                    | StringType ->
                        match opporator with
                        | Plus -> box ((left :?> string) + (right :?> string))
                        | NotEqual -> left.Equals right |> not |> box
                        | Equal -> left.Equals right
                        | _ -> ()

                    | AnyType ->
                        match opporator with
                        | NotEqual -> left.Equals right |> not |> box
                        | Equal -> left.Equals right
                        | _ -> ()

                (result, ids)

            | LiteralExpression value -> (value, ids)
            | IdentifierExpression id ->
               execute ids ids.[id]

            | AssignExpression (id, value) ->
                let (newValue, ids) = execute ids value
                ((), ids |> IdMap.update id { DataType = value.DataType; Type = (LiteralExpression(newValue))})

            | BindExpression (id, value) ->
                ((), ids |> IdMap.updateLocal id value)
                
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
                let (results, ids) = ids |> IdMap.useLocal (fun blockIds -> 
                    body
                    |> List.mapFold(fun ids expression -> execute ids expression) blockIds
                )
                (results |> List.last, ids)

            | IfExpression (condition, trueExpression, falseExpression) ->
                let condition = (execute ids condition |> fst) :?> bool
                if condition then
                    execute ids trueExpression
                else
                    match falseExpression with
                    | None -> ((), ids)
                    | Some falseExpression ->
                        execute ids falseExpression

            | LoopExpression (binding, condition, bodies) ->
                match binding with
                | Some binding ->
                    let mutable loopIds = execute (ids |> IdMap.openLocal) binding |> snd

                    let results = 
                        seq {
                            let mutable keepLooping = true
                            while keepLooping do 
                                let (condition, newLoopIds) = execute loopIds condition
                                if condition :?> bool |> not then
                                    keepLooping <- false
                                else
                                    let (results, newLoopIds) =
                                        bodies
                                        |> List.fold(fun state body -> 
                                            let (result, bodyIds) = state |> snd |> IdMap.useLocal (fun bodyIds -> execute bodyIds body)
                                            (result, bodyIds)
                            
                                        ) ((), newLoopIds)
                                    loopIds <- newLoopIds
                                    yield results
                                    ()
                        } |> Seq.toList
                    (results, loopIds |> IdMap.closeLocal)
                
            | FunctionExpression (paramater, body) ->
                ({ DataType = body.DataType
                   Type =
                       FrostFunction (fun ids argument ->
                            let argumentExpression = 
                                { DataType = paramater.Value
                                  Type = (LiteralExpression (argument)) }
                            execute (ids |> IdMap.updateLocal paramater.Id argumentExpression) body) }
                , ids)

            | NativeFunction call -> ({ DataType = expression.DataType; Type = FrostFunction (fun ids argument -> (call argument, ids)) }, ids)
            | FrostFunction _ -> failwith "Do not use FrostFunction, use NativeFunction Instead"

        expressions
        |> List.mapFold(fun ids expression -> execute ids expression) ([nativeFunctions |> Map] |> IdMap.ofList)
        |> fst
        |> List.last
