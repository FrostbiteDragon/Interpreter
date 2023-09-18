namespace FrostScript
open FrostScript.Core

module Validator =
    let validate : Validator = fun nativeFunctions nodes ->
        let mutable identifiers = 
            nativeFunctions 
            |> Seq.map (fun (key, expression) -> (key, (expression.DataType, false)))
            |> Map

        let rec validateNode node =

            let valueOrUnit (option : obj option) =
                match option with
                | Some value -> value
                | None -> ()

            match node with
            | BinaryNode (token, left, right) -> 
                { Token = token 
                  DataType = NumberType 
                  Type = BinaryExpression (validateNode left, validateNode right) }
            | LiteralNode token -> 
                match token.Type with
                | True -> { Token = token; DataType = BoolType; Type = LiteralExpression true }
                | False -> { Token = token; DataType = BoolType; Type = LiteralExpression false }
                | Number -> { Token = token; DataType = NumberType; Type = LiteralExpression (valueOrUnit token.Literal) }
                | String -> { Token = token; DataType = StringType; Type = LiteralExpression (valueOrUnit token.Literal) }
                | Id ->
                    let identifier = identifiers.TryFind token.Lexeme
                    match identifier with 
                    | Some (dataType, _) ->  
                        { Token = token 
                          DataType = dataType
                          Type = IdentifierExpression }
                    | None -> 
                        { Token = token
                          DataType = VoidType
                          Type = ValidationError "Identifier doesn't exists or is out of scope" }

                | _ -> failwith "unhandled literal type"
            | BindNode (token, isMutable, value) -> 
                let expression = validateNode value
                identifiers <- identifiers.Change(token.Lexeme, fun _ -> Some (expression.DataType, isMutable) )
                { Token = token
                  DataType = expression.DataType
                  Type = BindExpression expression }
            | ParserError (token, error) -> 
                { Token = token
                  DataType = VoidType
                  Type = ValidationError error }


        nodes
        |> List.map (fun x -> validateNode x)
