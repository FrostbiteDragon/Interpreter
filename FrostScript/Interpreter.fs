namespace FrostScript

module Interpreter =
    let interpret nativeFunctions expressions =
        let rec execute (ids : Expression IdMap) (expression : Expression) : obj * Expression IdMap =
            match expression.Type with
            | BinaryExpression (operator, leftExpression, rightExpression) ->
                let (left, ids) = execute ids leftExpression
                let (right, ids) = execute ids rightExpression

                let result =
                    match operator with
                    | Plus  -> 
                        match leftExpression.DataType with
                        | NumberType -> box ((left :?> double) + (right :?> double))
                        | StringType -> box ((left :?> string) + (right :?> string))
                        | _ -> ()
                    | Minus -> box ((left :?> double) - (right :?> double))
                    | Devide -> box ((left :?> double) / (right :?> double))
                    | Multiply  -> box ((left :?> double) * (right :?> double))
                    | GreaterThen     -> box ((left :?> double) > (right :?> double))
                    | GreaterOrEqual  -> box ((left :?> double) >= (right :?> double))
                    | LessThen        -> box ((left :?> double) < (right :?> double))
                    | LessOrEqual     -> box ((left :?> double) <= (right :?> double))

                    | And -> (left :?> bool) && (right :?> bool)
                    | Or       -> (left :?> bool) || (right :?> bool)
                    | NotEqual -> left.Equals right |> not |> box
                    | Equal    -> left.Equals right

                    | Pipe -> 
                        let func = right :?> FrostFunction;
                        func.Call ids left |> fst

                    | AccessorPipe -> 
                        let accessee = left :?> FrostObject
                        accessee.fields.[right :?> string]

                    | ObjectAccessor ->
                        let accessee = left :?> FrostObject
                        accessee.fields.[right :?> string]
               
                (result, ids)

            | LiteralExpression value -> (value, ids)
            | FieldExpression id -> (id, ids)
            | IdentifierExpression id ->
               execute ids ids.[id]

            | AssignExpression (id, value) ->
                let (newValue, ids) = execute ids value
                ((), ids |> IdMap.update id { DataType = value.DataType; Type = (LiteralExpression(newValue))})

            | BindExpression (id, value) ->
                ((), ids |> IdMap.updateLocal id value)
                
            | ValidationError (token, message) ->
                printfn "[Line:%i Character:%i] %s" token.Line token.Character message
                ((), ids)

            | CallExpression (callee, argument) ->
                let (callee, ids) = execute ids callee
                let (argument, ids) = execute ids argument
                let callee = callee :?> FrostFunction
                callee.Call ids argument

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
                | None -> 
                    let mutable loopIds = ids
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
                    (results, loopIds)

            | FunctionExpression (paramater, body) ->
                let call = 
                    fun ids argument ->
                        let argumentExpression = 
                            { DataType = paramater.Value
                              Type = (LiteralExpression (argument)) }
                        execute (ids |> IdMap.updateLocal paramater.Id argumentExpression) body
                
                ({ Type = expression.DataType; Call = call }, ids)

            | NativeFunction call -> 
                    ({Type = expression.DataType; Call = (fun ids argument -> (call argument, ids)) }, ids)

            | ObjectExpression fields -> ({ fields = fields |> Map.map (fun _ expression -> execute ids expression |> fst) }, ids)

        expressions
        |> List.mapFold(fun ids expression -> execute ids expression) ([nativeFunctions |> Map] |> IdMap.ofList)
        |> fst
        |> List.last
