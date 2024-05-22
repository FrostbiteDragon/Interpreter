module FrostScript.Features.Literal
    open FrostScript.Domain
    open FrostScript.Domain.Utilities

    let parse2 : ParserHandler = fun next ctx ->
        match (List.head ctx.Tokens).Type with
        | Number | String | Id | Void | Bool -> 
            Some { Node = { Token = ctx.Tokens.Head; Type = LiteralNode }; Tokens = ctx.Tokens |> skipOrEmpty 1}
        | _ -> next ctx

    //let parse : ParserSegment = fun (node, tokens) ->
    //    match (List.head tokens).Type with
    //    | Number | String | Id | Void | Bool -> 
    //        Success ({ Token = tokens.Head; Type = LiteralNode }, tokens |> skipOrEmpty 1)
    //    | _ -> NotFound
        
    //let validate : ValidatorFunction = fun next node ids ->
    //    match node.Type with
    //    | LiteralNode ->
    //        let token = node.Token
    //        let literalExpression = 
    //            match token.Type with
    //            | Bool   -> Ok { DataType = BoolType;   Type = LiteralExpression (valueOrUnit token.Literal) }
    //            | Number -> Ok { DataType = NumberType; Type = LiteralExpression (valueOrUnit token.Literal) }
    //            | String -> Ok { DataType = StringType; Type = LiteralExpression (valueOrUnit token.Literal) }
    //            | Void   -> Ok { DataType = VoidType;   Type = LiteralExpression () }
    //            | Id     ->
    //                match ids |> IdMap.tryFind token.Lexeme with 
    //                | Some (dataType, _) -> Ok { DataType = dataType; Type = IdentifierExpression token.Lexeme }
    //                | None               -> Error (token, $"Identifier \"{token.Lexeme}\" doesn't exist or is out of scope")
    //            | _      -> failwith "unhandled literal type"

    //        match literalExpression with
    //        | Ok expression -> Success (expression, ids)
    //        | Error (token, message) -> Failure (token, message, ids)
    //    | _ -> next node ids
        
