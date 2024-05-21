module FrostScript.Features.Literal
    open FrostScript.Domain
    open FrostScript.Domain.Utilities

    let parse : ParserFunction = fun next tokens ->
        match (List.head tokens).Type with
        | Number | String | Id | Void | Bool -> 
            (
                { Token = tokens.Head; Type = LiteralNode },  
                tokens |> skipOrEmpty 1
            )
        | _ -> next tokens
        
    let validate : ValidatorFunction = fun next node ids ->
        match node.Type with
        | LiteralNode ->
            let token = node.Token
            let literalExpression = 
                match token.Type with
                | Bool   -> { DataType = BoolType;   Type = LiteralExpression (valueOrUnit token.Literal) }
                | Number -> { DataType = NumberType; Type = LiteralExpression (valueOrUnit token.Literal) }
                | String -> { DataType = StringType; Type = LiteralExpression (valueOrUnit token.Literal) }
                | Void   -> { DataType = VoidType;   Type = LiteralExpression () }
                | Id     ->
                    match ids |> IdMap.tryFind token.Lexeme with 
                    | Some (dataType, _) -> { DataType = dataType; Type = IdentifierExpression token.Lexeme }
                    | None               -> { DataType = VoidType; Type = ValidationError (token, $"Identifier \"{token.Lexeme}\" doesn't exist or is out of scope") }
                | _      -> failwith "unhandled literal type"

            (literalExpression, ids)
        | _ -> next node ids
        
