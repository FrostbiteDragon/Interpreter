namespace FrostScript
open FrostScript.Core

module Validator =
    let validate : Validator = fun nativeFunctions nodes ->
        let mutable identifiers = 
            nativeFunctions 
            |> Seq.map (fun (key, expression) -> (key, (expression.DataType, false)))
            |> Map

        let error token message =
             { DataType = VoidType
               Type = ValidationError (token, message) }

        let expression token dataType expression =
            { DataType = dataType
              Type = expression }

        let rec validateNode node =
            let valueOrUnit (option : obj option) =
                match option with
                | Some value -> value
                | None -> ()

            match node with
            | BinaryNode (token, left, right) -> expression token NumberType (BinaryExpression (token.Type, validateNode left, validateNode right))

            | LiteralNode token -> 
                match token.Type with
                | True -> expression token BoolType (LiteralExpression true)
                | False -> expression token BoolType (LiteralExpression false)
                | Number -> expression token NumberType (LiteralExpression (valueOrUnit token.Literal))
                | String -> expression token StringType (LiteralExpression (valueOrUnit token.Literal))
                | Id ->
                    let identifier = identifiers.TryFind token.Lexeme
                    match identifier with 
                    | Some (dataType, _) -> expression token dataType (IdentifierExpression token.Lexeme)
                    | None -> error token "Identifier doesn't exist or is out of scope"

                | _ -> failwith "unhandled literal type"

            | BindNode (token, id, isMutable, value) -> 
                let value = validateNode value
                identifiers <- identifiers.Change(token.Lexeme, fun _ -> Some (value.DataType, isMutable) )
                expression token value.DataType (BindExpression(id, value))

            | ParserError (token, message) -> error token message

            | AssignNode (token, id, value) ->
                let identifier = identifiers.TryFind id
                match identifier with
                | Some (dataType, isMutable) -> 
                    if isMutable then expression token dataType (AssignExpression (id, validateNode value))
                    else error token "Varriable is not mutable"
                | None -> error token "Identifier doesn't exist or is out of scope"

            | CallNode (token, callee, argument) ->
                let callee = validateNode callee
                let argument = validateNode argument
                match callee.DataType with
                | FunctionType (inputType, outputType) ->
                    if inputType = argument.DataType || inputType = AnyType then
                        expression token outputType (CallExpression (callee, argument))
                    else
                        error token $"Function expected argument of type {inputType} but was given an argument of type {argument.DataType} instead"
                | _ -> error token $"{token.Lexeme} is not callable"
                        
        nodes
        |> List.map (fun x -> validateNode x)
