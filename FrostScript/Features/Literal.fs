[<AutoOpen>]
module FrostScript.Features.Literal
    open FrostScript.Domain
    open Utilities

    let literal = { 
        Lexer = fun ctx ->
            match ctx.Characters with
            | [] -> Ok ctx |> Some
            | char :: _ ->
                match char with
                | letter when System.Char.IsLetter letter ->
                    let word = 
                        new string (ctx.Characters
                        |> List.takeWhile (fun x -> System.Char.IsLetterOrDigit(x))
                        |> List.toArray)

                    match word with
                    | "true"   -> 
                        { Type = Bool; Lexeme = "true"; Literal = Some (true); Position = ctx.Position }
                        |> addToken ctx word.Length
                    | "false"  ->
                        { Type = Bool; Lexeme = "false"; Literal = Some (false); Position = ctx.Position }
                        |> addToken ctx word.Length
                    | _  ->
                        { Type = Id; Lexeme = word; Literal = None; Position = ctx.Position }
                        |> addToken ctx word.Length

                | number when System.Char.IsDigit number ->
                    let integerDigits = 
                        ctx.Characters
                        |> List.takeWhile (fun x -> System.Char.IsDigit x)

                    let fractionalDigits =
                        let characters = ctx.Characters |> skipOrEmpty integerDigits.Length
                        if characters <> [] && characters.Head = '.' then
                            characters
                            |> List.skip (1)
                            |> List.takeWhile (fun x -> System.Char.IsDigit x)
                            |> List.append ['.']
                        else []

                    let number = 
                        integerDigits @ fractionalDigits
                        |> List.toArray 
                        |> System.String
                    { Type = Number; Lexeme = number; Literal = Some (double number); Position = ctx.Position }
                    |> addToken ctx number.Length
               
                | _ -> None

        Parser = fun next ctx ->
            match (List.head ctx.Tokens).Type with
            | Number | String | Id | Void | Bool -> 
                Ok { Node = { Token = ctx.Tokens.Head; Type = LiteralNode }; Tokens = ctx.Tokens |> skipOrEmpty 1 }
            | _ -> next ctx

        Validator = fun _ ctx ->
            match ctx.Node.Type with
            | LiteralNode ->
                let token = ctx.Node.Token
                let literalExpression = 
                    match token.Type with
                    | Bool   -> Ok { DataType = BoolType;   Type = LiteralExpression (Option.toObj token.Literal) }
                    | Number -> Ok { DataType = NumberType; Type = LiteralExpression (Option.toObj token.Literal) }
                    | String -> Ok { DataType = StringType; Type = LiteralExpression (Option.toObj token.Literal) }
                    | Void   -> Ok { DataType = VoidType;   Type = LiteralExpression () }
                    | Id     ->
                        match ctx.Ids |> IdMap.tryFind token.Lexeme with
                        | Some (dataType, _) -> Ok { DataType = dataType; Type = IdentifierExpression token.Lexeme }
                        | None               -> Error [(token, $"Identifier \"{token.Lexeme}\" doesn't exist or is out of scope")]
                    | _      -> failwith "unhandled literal type"

                literalExpression
                |> Result.map (fun expression -> { Expression = expression; Ids = ctx.Ids })
                |> Some
            | _ -> None

        Interpreter = fun _ ctx ->
            match ctx.Expression.Type with
            | LiteralExpression value -> Ok { Value = value; Ids = ctx.Ids } |> Some
            | _ -> None
    }