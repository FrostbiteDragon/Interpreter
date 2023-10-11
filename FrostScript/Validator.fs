namespace FrostScript

module Validator =
    let validate nativeFunctions nodes =
        let rec validateNode (identifiers : IdentifierMap<DataType * bool>) node : Expression * IdentifierMap<DataType * bool> =
            let error token message =
                 { DataType = VoidType
                   Type = ValidationError (token, message) }

            let expression dataType expression =
                { DataType = dataType
                  Type = expression }

            match node with
            | BinaryNode (token, left, right) -> 
                let (left, identifiers) = validateNode identifiers left
                let (right, identifiers) = validateNode identifiers right

                let dataType = 
                    match token.Type with
                    | LessThen 
                    | LessOrEqual 
                    | GreaterThen 
                    | GreaterOrEqual 
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
                    | NotEqual -> Some BoolType

                    | And
                    | Or ->
                        if left.DataType = BoolType && right.DataType = BoolType then Some BoolType
                        else None

                    | _ -> failwith "unhandled opporator"

                match dataType with
                | None -> (error token $"Opporator '{token.Lexeme}' cannot be used between types {left.DataType} and {right.DataType}", identifiers)
                | Some dataType -> (expression dataType (BinaryExpression (token.Type, left, right)), identifiers)

            | BindNode (_, id, isMutable, value) -> 
                let (value, identifiers) = validateNode identifiers value
                let bindEpression = expression value.DataType (BindExpression(id, value))

                (bindEpression, identifiers.ChangeLocal id (value.DataType, isMutable))

            | AssignNode (token, id, value) ->
                let identifier = identifiers.TryFind id
                match identifier with
                | Some (dataType, isMutable) -> 
                    let (newValue, _) = validateNode identifiers value
                    if isMutable then
                        if dataType <> newValue.DataType then (error token $"Varriable of type \"{dataType}\" cannot be assigned a value of type \"{newValue.DataType}\"", identifiers)
                        else (expression dataType (AssignExpression (id, newValue)), identifiers.Change id (newValue.DataType, isMutable))
                    else 
                        (error token "Varriable is not mutable", identifiers)
                | None -> (error token $"Identifier \"{id}\" doesn't exist or is out of scope", identifiers)
                
            | BlockNode (_, body) ->
                let (expressions, ids) =
                    body
                    |> List.mapFold (fun identifiers node -> validateNode identifiers node) {globalIds = identifiers.Ids; localIds = Map.empty}

                (expression ((expressions |> List.last).DataType) (BlockExpression expressions), {ids with localIds = ids.globalIds})

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
                        match identifiers.TryFind token.Lexeme with 
                        | Some (dataType, _) -> expression dataType (IdentifierExpression token.Lexeme)
                        | None -> error token $"Identifier \"{token.Lexeme}\" doesn't exist or is out of scope"
                    | _      -> failwith "unhandled literal type"

                (literalExpression, identifiers)

            | CallNode (token, callee, argument) ->
                let (callee, _) = validateNode identifiers callee
                let (argument, _) = validateNode identifiers argument
                match callee.DataType with
                | FunctionType (inputType, outputType) ->
                    if inputType = argument.DataType || inputType = AnyType then
                        (expression outputType (CallExpression (callee, argument)), identifiers)
                    else
                        (error token $"Function expected argument of type {inputType} but was given an argument of type {argument.DataType} instead", identifiers)
                | _ -> (error token $"{token.Lexeme} is not callable", identifiers)
               
            | FunctionNode (_, parameter, body) ->
                let functionIdentifiers = {globalIds = identifiers.Ids; localIds = Map [parameter.Id, (parameter.Value, false)]}
                let (body, identifiers) = validateNode functionIdentifiers body
                (
                    expression (FunctionType(parameter.Value, body.DataType)) (FunctionExpression({Id = parameter.Id; Value = parameter.Value}, body)), 
                    {identifiers with localIds = identifiers.globalIds}
                )

            | IfNode (token, condition, trueNode, falseNode) ->
                let (condition, identifiers) = validateNode identifiers condition
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
                let loopIds = {globalIds = identifiers.Ids; localIds = Map.empty}
                match binding with
                | Some binding ->
                    match binding with
                    | BindNode _ ->
                        let (binding, loopIds) = validateNode loopIds binding
                        let (condition, loopIds) = validateNode loopIds condition

                        if condition.DataType <> BoolType then (error token $"The expression following 'while' must be of type {BoolType}", identifiers)
                        else 
                            let (bodies, loopIds) = 
                                bodies
                                |> List.mapFold (fun (loopIds : IdentifierMap<DataType * bool>) body -> 
                                    let bodyIds = {globalIds = loopIds.Ids; localIds = Map.empty}
                                    let (body, bodyIds) = validateNode bodyIds body
                                    (body, {bodyIds with localIds = bodyIds.globalIds})
                                ) loopIds
                            (expression ((bodies |> List.last).DataType) (LoopExpression(Some binding, condition, bodies)), { identifiers with localIds = loopIds.globalIds })
                    | _ -> (error token "The expression following 'for' must be a binding", identifiers)
                | None -> failwith "not implemented"
                    

            | ParserError (token, message) -> (error token message, identifiers)
        
        let nativeFunctions = nativeFunctions |> Seq.map (fun (key, value) -> (key, (value.DataType, false))) |> Map.ofSeq
        nodes
        |> List.mapFold (fun identifiers node -> validateNode identifiers node) {globalIds = Map.empty; localIds = nativeFunctions }
        |> fst
        

