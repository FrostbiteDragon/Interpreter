namespace FrostScript

module Lexer =
    let lex (rawScript : string) =
        let chars = rawScript.ToCharArray() |> Array.toList
        seq { 
            let mutable character = 0
            let mutable line = 1
            let mutable i = -1
           
            while i < chars.Length - 1 do
                character <- character + 1
                i <- i + 1
                match chars.[i] with
                | ')' -> yield { Type = ParentheseClose; Lexeme = ")"; Literal = None; Line = line; Character = character }
                | '{' -> yield { Type = BraceOpen; Lexeme = "{"; Literal = None; Line = line; Character = character }
                | '}' -> yield { Type = BraceClose; Lexeme = "}"; Literal = None; Line = line; Character = character }
                | ',' -> yield { Type = Comma; Lexeme = ","; Literal = None; Line = line; Character = character }
                | '.' -> yield { Type = Period; Lexeme = "."; Literal = None; Line = line; Character = character }
                | '+' -> yield { Type = Operator Plus; Lexeme = "+"; Literal = None; Line = line; Character = character }
                | '*' -> yield { Type = Operator Multiply; Lexeme = "*"; Literal = None; Line = line; Character = character }
                | '/' -> yield { Type = Operator Devide; Lexeme = "/"; Literal = None; Line = line; Character = character }
                | ';' -> yield { Type = SemiColon; Lexeme = ";"; Literal = None; Line = line; Character = character }
                | ':' -> yield { Type = Colon; Lexeme = ":"; Literal = None; Line = line; Character = character }
                | '>' ->
                    yield
                         match chars.[i + 1] with
                            | '=' ->
                                i <- i + 1
                                character <- character + 1
                                { Type = GreaterOrEqual; Lexeme = ">="; Literal = None; Line = line; Character = character }
                            | _ -> 
                                { Type = GreaterThen; Lexeme = ">"; Literal = None; Line = line; Character = character }

                | '<' -> 
                    yield
                        match chars.[i + 1] with
                        | '=' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = LessOrEqual; Lexeme = "<="; Literal = None; Line = line; Character = character }
                        | _ -> 
                            { Type = LessThen; Lexeme = "<"; Literal = None; Line = line; Character = character }
                | '!' -> 
                    yield 
                        match chars.[i + 1] with
                        | '=' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = NotEqual; Lexeme = "!="; Literal = None; Line = line; Character = character }
                        | _ -> 
                            { Type = Not; Lexeme = "!"; Literal = None; Line = line; Character = character }
                            
                | '=' ->
                    yield 
                        match chars.[i + 1] with
                        | '=' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = Equal; Lexeme = "=="; Literal = None; Line = line; Character = character }
                        | _ -> 
                            { Type = Assign; Lexeme = "="; Literal = None; Line = line; Character = character }
                | '|' -> 
                    yield 
                        match chars.[i + 1] with
                        | '>' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = BlockReturn; Lexeme = "|>"; Literal = None; Line = line; Character = character }
                        | '-' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = Pipe; Lexeme = "|-"; Literal = None; Line = line; Character = character }
                        | '.' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = ObjectPipe; Lexeme = "|."; Literal = None; Line = line; Character = character }
                        | _ -> 
                            { Type = BlockOpen; Lexeme = "|"; Literal = None; Line = line; Character = character }
                | '(' -> 
                    yield 
                        match chars.[i + 1] with
                        | ')' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = Void; Lexeme = "()"; Literal = None; Line = line; Character = character }
                        | _ -> 
                            { Type = ParentheseOpen; Lexeme = "("; Literal = None; Line = line; Character = character }

                | '-' -> 
                    yield 
                        match chars.[i + 1] with
                        | '>' ->
                            i <- i + 1
                            character <- character + 1
                            { Type = Arrow; Lexeme = "->"; Literal = None; Line = line; Character = character }
                        | _ -> 
                            { Type = Operator Minus; Lexeme = "-"; Literal = None; Line = line; Character = character }

                //strings
                | '"' ->
                    if chars
                        |> List.skip (i + 1)
                        |> List.contains '"'
                        |> not
                    then
                        yield { Type = LexerError "String never closed"; Lexeme = string chars.[i]; Literal = None; Line = line; Character = character } 
                        i <- chars.Length - 1
                    else
                        let word = 
                            new string (chars 
                            |> List.skip (i + 1) 
                            |> List.takeWhile (fun x -> x <> '"')
                            |> List.toArray)

                        yield {Type = String; Lexeme = word; Literal = Some word; Line = line; Character = character }
                        i <- i + word.Length + 1
                        character <- character + word.Length + 1

                //labels
                | _ when System.Char.IsLetter chars.[i] ->
                    let word = 
                        new string (chars
                        |> List.skip i
                        |> List.takeWhile (fun x -> System.Char.IsLetterOrDigit(x))
                        |> List.toArray)

                    match word with
                    | "true"   -> yield { Type = Bool; Lexeme = word; Literal = Some true; Line = line; Character = character }
                    | "false"  -> yield { Type = Bool; Lexeme = word; Literal = Some false; Line = line; Character = character }
                    | "let"    -> yield { Type = Let; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "var"    -> yield { Type = Var; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "fun"    -> yield { Type = Fun; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "or"     -> yield { Type = Or; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "and"    -> yield { Type = And; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "num"    -> yield { Type = TypeAnnotation (NumberType); Lexeme = word; Literal = None; Line = line; Character = character }
                    | "any"    -> yield { Type = TypeAnnotation (AnyType); Lexeme = word; Literal = None; Line = line; Character = character }
                    | "void"   -> yield { Type = TypeAnnotation (VoidType); Lexeme = word; Literal = None; Line = line; Character = character }
                    | "bool"   -> yield { Type = TypeAnnotation (BoolType); Lexeme = word; Literal = None; Line = line; Character = character }
                    | "string" -> yield { Type = TypeAnnotation (StringType); Lexeme = word; Literal = None; Line = line; Character = character }
                    | "if"     -> yield { Type = If; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "else"   -> yield { Type = Else; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "for"    -> yield { Type = For; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "while"  -> yield { Type = While; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "do"     -> yield { Type = Do; Lexeme = word; Literal = None; Line = line; Character = character }
                    | "new"     -> yield { Type = New; Lexeme = word; Literal = None; Line = line; Character = character }
                    | _        -> yield { Type = Id; Lexeme = word; Literal = None; Line = line; Character = character }

                    i <- i + word.Length - 1
                    character <- character + word.Length

                //numbers
                | _ when System.Char.IsDigit chars.[i] ->
                    let getNumber chars = 
                        chars
                        |> List.takeWhile (fun x -> System.Char.IsDigit x)
                        |> List.toArray

                    let wholeNumber = getNumber (chars |> List.skip i)

                    let number = 
                        if chars.Length > i + wholeNumber.Length && chars.[i + wholeNumber.Length] = '.' then
                            let decimals = getNumber (chars |> List.skip (i + wholeNumber.Length + 1))
                            new string(
                                wholeNumber
                                |> Array.append [|'.'|]
                                |> Array.append decimals
                                |> Array.rev
                            )
                        else
                            new string(wholeNumber)

                    i <- i + number.Length - 1
                    character <- character + number.Length - 1

                    yield { Type = Number; Lexeme = number; Literal = Some (double number); Line = line; Character = character }

                //ignore whitespace
                | ' ' | '\t' -> ()

                //new line
                | '\r' -> 
                    if chars.[i + 1] = '\n' then
                        i <- i + 1
                        line <- line + 1
                        character <- 0
                    else
                        line <- line + 1
                        character <- 0
                        
                | '\n' -> 
                    line <- line + 1
                    character <- 0

                | _ -> yield { Type = LexerError "Unexpected character"; Lexeme = string chars.[i]; Literal = None; Line = line; Character = character } 

        } |> Seq.toList