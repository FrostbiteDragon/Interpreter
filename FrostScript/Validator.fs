namespace FrostScript

module Validator =
    let validate nativeFunctions nodes =
        let rec validateNode (ids : (DataType * bool) idMap) node : Expression * (DataType * bool) idMap =
            let error token message =
                 { DataType = VoidType
                   Type = ValidationError (token, message) }

            let expression dataType expression =
                { DataType = dataType
                  Type = expression }

            match node with
            | BinaryNode (token, left, right) -> 
                let (left, identifiers) = validateNode ids left
                let (right, identifiers) = validateNode identifiers right

                let dataType = 
                    match token.Type with
                    | Star
                    | Slash 
                    | Minus ->
                        if left.DataType = NumberType && right.DataType = NumberType then Some NumberType
                        else None

                    | Plus ->
                        if left.DataType = NumberType && right.DataType = NumberType then Some NumberType
                        else if left.DataType = StringType && right.DataType = StringType then Some StringType
                        else None

                    | Equal
                    | NotEqual 
                    | LessThen 
                    | LessOrEqual 
                    | GreaterThen 
                    | GreaterOrEqual -> Some BoolType

                    | And
                    | Or ->
                        if left.DataType = BoolType && right.DataType = BoolType then Some BoolType
                        else None

                    | _ -> failwith "unhandled opporator"

                match dataType with
                | None -> (error token $"Opporator '{token.Lexeme}' cannot be used between types {left.DataType} and {right.DataType}", identifiers)
                | Some dataType -> (expression dataType (BinaryExpression (token.Type, left, right)), identifiers)

            | BindNode (_, id, isMutable, value) -> 
                let (value, ids) = validateNode ids value
                let bindEpression = expression value.DataType (BindExpression(id, value))

                (bindEpression, IdMap.updateLocal id (value.DataType, isMutable) ids)

            | AssignNode (token, id, value) ->
                let identifier = ids |> IdMap.tryFind id
                match identifier with
                | Some (dataType, isMutable) -> 
                    let (newValue, _) = validateNode ids value
                    if isMutable then
                        if dataType <> newValue.DataType then (error token $"Varriable of type \"{dataType}\" cannot be assigned a value of type \"{newValue.DataType}\"", ids)
                        else (expression dataType (AssignExpression (id, newValue)), ids |> IdMap.update id (newValue.DataType, isMutable))
                    else 
                        (error token "Varriable is not mutable", ids)
                | None -> (error token $"Identifier \"{id}\" doesn't exist or is out of scope", ids)
                
            | BlockNode (_, body) ->
                let (expressions, ids) = ids |> IdMap.useLocal (fun blockIds -> 
                    body
                    |> List.mapFold (fun identifiers node -> validateNode identifiers node) blockIds
                )

                (expression ((expressions |> List.last).DataType) (BlockExpression expressions), ids)

            | LiteralNode token -> 
                let valueOrUnit (option : obj option) =
                    match option with
                    | Some value -> value
                    | None -> ()

                let literalExpression = 
                    match token.Type with
                    | Bool   -> expression BoolType   (LiteralExpression (valueOrUnit token.Literal))
                    | Number -> expression NumberType (LiteralExpression (valueOrUnit token.Literal))
                    | String -> expression StringType (LiteralExpression (valueOrUnit token.Literal))
                    | Void   -> expression VoidType   (LiteralExpression ())
                    | Id     ->
                        match ids |> IdMap.tryFind token.Lexeme with 
                        | Some (dataType, _) -> expression dataType (IdentifierExpression token.Lexeme)
                        | None -> error token $"Identifier \"{token.Lexeme}\" doesn't exist or is out of scope"
                    | _      -> failwith "unhandled literal type"

                (literalExpression, ids)

            | CallNode (token, callee, argument) ->
                let (callee, _) = validateNode ids callee
                let (argument, _) = validateNode ids argument
                match callee.DataType with
                | FunctionType (inputType, outputType) ->
                    if inputType = argument.DataType || inputType = AnyType then
                        (expression outputType (CallExpression (callee, argument)), ids)
                    else
                        (error token $"Function expected argument of type {inputType} but was given an argument of type {argument.DataType} instead", ids)
                | _ -> (error token $"{token.Lexeme} is not callable", ids)
               
            | FunctionNode (_, parameter, body) ->
                let (body, ids) = ids |> IdMap.useLocal (fun functionIds -> 
                    let functionIdentifiers = functionIds |> IdMap.updateLocal parameter.Id (parameter.Value, false)
                    validateNode functionIdentifiers body
                )
               
                (
                    expression (FunctionType(parameter.Value, body.DataType)) (FunctionExpression({Id = parameter.Id; Value = parameter.Value}, body)), 
                    ids
                )

            | IfNode (token, condition, trueNode, falseNode) ->
                let (condition, identifiers) = validateNode ids condition
                if condition.DataType <> BoolType then (error token "If condition must be of type bool", identifiers)
                else 
                    let (trueExpression, identifiers) = validateNode identifiers trueNode
                    match falseNode with
                    | Some falseNode -> 
                        let (falseExpression, identifiers) = validateNode identifiers falseNode
                        if trueExpression.DataType <> falseExpression.DataType then
                            (error token $"Both casses of an If expression must return an expression of the same datatype. The types present are {trueExpression.DataType} and {falseExpression.DataType}", identifiers)
                        else
                            (expression trueExpression.DataType (IfExpression(condition, trueExpression, Some falseExpression)), identifiers)

                    | None -> 
                        if trueExpression.DataType = VoidType then (expression trueExpression.DataType (IfExpression(condition, trueExpression, None)), identifiers)
                        else (error token "If expressions that do not return void must have an else clause", identifiers)

            | LoopNode (token, binding, condition, bodies) ->
                match binding with
                | Some binding ->
                    match binding with
                    | BindNode _ ->
                        ids |> IdMap.useLocal (fun loopIds ->
                            let (binding, loopIds) = validateNode loopIds binding
                            let (condition, loopIds) = validateNode loopIds condition

                            if condition.DataType <> BoolType then (error token $"The expression following 'while' must be of type {BoolType}", ids)
                            else 
                                let (bodies, loopIds) = 
                                    bodies
                                    |> List.mapFold (fun (loopIds : (DataType * bool) idMap) body -> 
                                        loopIds |> IdMap.useLocal (fun bodyIds -> validateNode bodyIds body)
                                    ) loopIds
                                (expression ((bodies |> List.last).DataType) (LoopExpression(Some binding, condition, bodies)), loopIds)
                        )
                    | _ -> (error token "The expression following 'for' must be a binding", ids)
                | None -> failwith "not implemented"
                    

            | ParserError (token, message) -> (error token message, ids)
        
        let nativeFunctions = nativeFunctions |> Seq.map (fun (key, value) -> (key, (value.DataType, false))) |> Map.ofSeq
        nodes
        |> List.mapFold (fun identifiers node -> validateNode identifiers node) ([nativeFunctions] |> IdMap.ofList)
        |> fst
        

