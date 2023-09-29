namespace FrostScript
open FrostScript.Core

module Validator =
    let validate : Validator = fun nativeFunctions nodes ->
        let rec validateNode (identifiers : IdentifierMap<DataType * bool>) node : Expression * IdentifierMap<DataType * bool> =
            let error token message =
                 { DataType = VoidType
                   Type = ValidationError (token, message) }

            let expression dataType expression =
                { DataType = dataType
                  Type = expression }

            match node with
            | BinaryNode (token, left, right) -> 
                let (left, _) = validateNode identifiers left
                let (right, _) = validateNode identifiers right
                (expression NumberType (BinaryExpression (token.Type, left, right)), identifiers)

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
                    | True   -> expression BoolType   (LiteralExpression true)
                    | False  -> expression BoolType   (LiteralExpression false)
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
               

            | ParserError (token, message) -> (error token message, identifiers)
        
        let nativeFunctions = nativeFunctions |> Seq.map (fun (key, value) -> (key, (value.DataType, false))) |> Map.ofSeq
        nodes
        |> List.mapFold (fun identifiers node -> validateNode identifiers node) {globalIds = Map.empty; localIds = nativeFunctions }
        |> fst
        

