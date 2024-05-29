[<AutoOpen>]
module FrostScript.Features.Literal
    open FrostScript.Domain

    let lex : LexFunc = fun ctx ->
        match ctx.Characters with
        | [] -> Some (Ok ctx)
        | char :: tail ->
            match char with
            | '1' -> Some (Ok {ctx with Tokens = (ctx.Tokens @ [{ Type = Number; Lexeme = "1"; Literal = Some (double 1); Position = ctx.Position }]) })
            | _ -> None

    let parse : ParseFunc = fun ctx ->
        match (List.head ctx.Tokens).Type with
        | Number | String | Id | Void | Bool -> 
            Ok { Node = { Token = ctx.Tokens.Head; Type = LiteralNode }; Tokens = ctx.Tokens |> skipOrEmpty 1 }
        | _ -> Ok ctx

    let validate : ValidationFunc = fun ctx ->
        match ctx.Node.Type with
        | LiteralNode ->
            let token = ctx.Node.Token
            let literalExpression = 
                match token.Type with
                | Bool   -> Ok { DataType = BoolType;   Type = LiteralExpression (valueOrUnit token.Literal) }
                | Number -> Ok { DataType = NumberType; Type = LiteralExpression (valueOrUnit token.Literal) }
                | String -> Ok { DataType = StringType; Type = LiteralExpression (valueOrUnit token.Literal) }
                | Void   -> Ok { DataType = VoidType;   Type = LiteralExpression () }
                | Id     ->
                    match ctx.Ids |> IdMap.tryFind token.Lexeme with 
                    | Some (dataType, _) -> Ok { DataType = dataType; Type = IdentifierExpression token.Lexeme }
                    | None               -> Error (token, $"Identifier \"{token.Lexeme}\" doesn't exist or is out of scope")
                | _      -> failwith "unhandled literal type"

            match literalExpression with
            | Ok expression -> Some (Ok expression)
            | Error (token, message) -> Some (Error [(token, message)])
        | _ -> None
        