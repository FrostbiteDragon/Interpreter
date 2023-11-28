namespace FrostScript

module Validator =
    let validate nativeFunctions nodes =
        let rec validateNode (ids : (DataType * bool) idMap) (node : Node) : Expression * (DataType * bool) idMap =
            let error token message =
                 { DataType = VoidType
                   Type = ValidationError (token, message) }

            let expression dataType expression =
                { DataType = dataType
                  Type = expression }

            let token = node.Token

            match node.Type with
            | BinaryNode (operator, left, right) -> 

                let binaryExpression dataType left right = expression dataType (BinaryExpression (operator, left, right))
                let invalidDataTypeError leftType rightType = error token  $"Opporator '{token.Lexeme}' cannot be used between types {leftType} and {rightType}"
                match operator with
                | Multiply
                | Devide 
                | Minus ->
                    let (left, ids) = validateNode ids left
                    let (right, ids) = validateNode ids right
                    
                    if left.DataType = NumberType && right.DataType = NumberType then (binaryExpression NumberType left right, ids)
                    else (invalidDataTypeError left right, ids)

                | Plus ->
                    let (left, ids) = validateNode ids left
                    let (right, ids) = validateNode ids right

                    if left.DataType = NumberType && right.DataType = NumberType then (binaryExpression NumberType left right, ids)
                    else if left.DataType = StringType && right.DataType = StringType then (binaryExpression NumberType left right, ids)
                    else (invalidDataTypeError left right, ids)

                | Equal
                | NotEqual 
                | LessThen 
                | LessOrEqual 
                | GreaterThen 
                | GreaterOrEqual -> 
                    let (left, ids) = validateNode ids left
                    let (right, ids) = validateNode ids right

                    (expression BoolType (BinaryExpression (operator, left, right)), ids)

                | And
                | Or ->
                    let (left, ids) = validateNode ids left
                    let (right, ids) = validateNode ids right

                    if left.DataType = BoolType && right.DataType = BoolType then (binaryExpression BoolType left right, ids)
                    else (invalidDataTypeError left right, ids)

                | Pipe ->
                    let (left, ids) = validateNode ids left
                    let (right, ids) = validateNode ids right

                    match right.DataType with
                    | FunctionType (inputType, outputType) ->
                        if left.DataType <> inputType && inputType <> AnyType then (error token $"Can not pipe value {left.DataType} into function with input {inputType}", ids)
                        else (binaryExpression outputType left right, ids)
                    | _ -> (error token "Right side of pipe operator must be a function", ids)

                | AccessorPipe ->
                    let (left, ids) = validateNode ids left

                    match left.DataType with
                    | ObjectType (fields) ->
                        let mutable dataType = VoidType 
                        let exists = fields.TryGetValue (right.Token.Lexeme, &dataType)

                        if exists |> not then (error token $"Object does not contain the field \"{right.Token.Lexeme}\"", ids)
                        else (binaryExpression dataType left (expression dataType (FieldExpression right.Token.Lexeme)), ids)
                    | _ -> (error token "Left side of Pipe must be an object", ids)

                | ObjectAccessor ->
                    let (left, ids) = validateNode ids left

                    match left.Type with
                    | ValidationError _ -> (left, ids)
                    | _ ->
                        match left.DataType with 
                        | ObjectType fields ->
                            let mutable dataType = VoidType 
                            let exists = fields.TryGetValue (right.Token.Lexeme, &dataType)

                            if exists then (binaryExpression dataType left (expression dataType (FieldExpression right.Token.Lexeme)), ids)
                            else (error token $"Object does not contain the field \"{right.Token.Lexeme}\"", ids)
                        | _ -> (error token "Expression leading '.' must be of type object", ids)

            | BindNode (id, isMutable, value) -> 
                match value.Type with
                | ParserError (message)-> (error value.Token message, ids)
                | _ ->
                    let (value, ids) = validateNode ids value
                    let bindEpression = expression value.DataType (BindExpression(id, value))

                    (bindEpression, IdMap.updateLocal id (value.DataType, isMutable) ids)

            | AssignNode (id, value) ->
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
                
            | BlockNode body ->
                let (expressions, ids) = ids |> IdMap.useLocal (fun blockIds -> 
                    body
                    |> List.mapFold (fun identifiers node -> validateNode identifiers node) blockIds
                )

                (expression ((expressions |> List.last).DataType) (BlockExpression expressions), ids)

            | LiteralNode -> 
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

            | CallNode (callee, argument) ->
                let (callee, _) = validateNode ids callee
                let (argument, _) = validateNode ids argument
                match callee with
                | { DataType = FunctionType (inputType, outputType) } ->
                    if inputType = argument.DataType || inputType = AnyType then
                        (expression outputType (CallExpression (callee, argument)), ids)
                    else
                        (error token $"Function expected argument of type {inputType} but was given an argument of type {argument.DataType} instead", ids)

                | { Type = ValidationError _ }-> (callee, ids)
                | _ -> (error token $"The value {argument} could not be applied since the preceding expression is not callable", ids)
               
            | FunctionNode (parameter, body) ->
                let (body, ids) = ids |> IdMap.useLocal (fun functionIds -> 
                    let functionIdentifiers = functionIds |> IdMap.updateLocal parameter.Id (parameter.Value, false)
                    validateNode functionIdentifiers body
                )
               
                (
                    expression (FunctionType(parameter.Value, body.DataType)) (FunctionExpression({Id = parameter.Id; Value = parameter.Value}, body)), 
                    ids
                )

            | IfNode (condition, trueNode, falseNode) ->
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

            | LoopNode (binding, condition, bodies) ->
                let validateCondition ids =
                    let (condition, ids) = validateNode ids condition

                    if condition.DataType <> BoolType then (Error $"The expression following 'while' must be of type {BoolType}", ids)
                    else (Ok condition, ids)

                let validateBodies ids =
                    let (bodies, ids) = 
                        bodies
                        |> List.mapFold (fun (loopIds : (DataType * bool) idMap) body -> 
                            loopIds |> IdMap.useLocal (fun bodyIds -> validateNode bodyIds body)
                        ) ids
                    (Ok bodies, ids)

                match binding with
                | Some binding ->
                    match binding.Type with
                    | BindNode _ ->
                        ids |> IdMap.useLocal (fun loopIds ->
                            let (binding, loopIds) = validateNode loopIds binding
                            let (condition, loopIds) = validateCondition loopIds
                            let (bodies, loopIds) = validateBodies loopIds
                    
                            match bodies with
                            | Error message -> (error token message, ids)
                            | Ok bodies -> 
                                match condition with
                                | Error message -> (error token message, ids)
                                | Ok condition -> (expression ((bodies |> List.last).DataType) (LoopExpression(Some binding, condition, bodies)), loopIds))

                    | _ -> (error token "The expression following 'for' must be a binding", ids)

                | None -> 
                    let (condition, ids) = validateCondition ids
                    let (bodies, ids) = validateBodies ids
                    
                    match bodies with
                    | Error message -> (error token message, ids)
                    | Ok bodies -> 
                        match condition with
                        | Error message -> (error token message, ids)
                        | Ok condition -> (expression ((bodies |> List.last).DataType) (LoopExpression(None, condition, bodies)), ids)
                        
            | ObjectNode fields ->
                let fields = fields |> Map.map(fun _ y -> (validateNode ids y) |> fst)
                (expression (ObjectType(fields |> Map.map(fun _ y -> y.DataType))) (ObjectExpression fields), ids)

            | ParserError message -> (error token message, ids)
            | Stop -> failwith "Stop is not a valid node, there is likely a parsing error"
        
        let nativeFunctions = nativeFunctions |> Seq.map (fun (key, value) -> (key, (value.DataType, false))) |> Map.ofSeq
        nodes
        |> List.mapFold (fun identifiers node -> validateNode identifiers node) ([nativeFunctions] |> IdMap.ofList)
        |> fst
        

