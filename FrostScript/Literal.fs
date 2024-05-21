namespace FrostScript
    module Literal =
        let validate (next: ValidatorFunction) : ValidatorFunction = fun (node, ids) ->
            let error token message =
                 { DataType = VoidType
                   Type = ValidationError (token, message) }

            let expression dataType expression =
                { DataType = dataType
                  Type = expression }

            let valueOrUnit (option : obj option) =
                match option with
                | Some value -> value
                | None -> ()

            match node.Type with
            | LiteralNode ->

                let token = node.Token
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
            | _ -> next (node, ids)
        
