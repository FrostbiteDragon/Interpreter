namespace FrostScript
open FrostScript.Core

module Lexer =
    let lex : Lexer = fun rawScript ->
        let chars = rawScript.ToCharArray() |> Array.toList
        seq { 
            let mutable character = 0
            let mutable line = 1
            let mutable i = -1
            while i < chars.Length - 1 do
                character <- character + 1
                i <- i + 1
                match chars.[i] with
                | '(' -> yield {Type = ParentheseOpen; Lexeme = "("; Literal = None; Line = line; Character = character}
                | ')' -> yield {Type = ParentheseClose; Lexeme = ")"; Literal = None; Line = line; Character = character}
                | '{' -> yield {Type = BraceOpen; Lexeme = "{"; Literal = None; Line = line; Character = character}
                | '}' -> yield {Type = BraceClose; Lexeme = "}"; Literal = None; Line = line; Character = character}
                | ',' -> yield {Type = Comma; Lexeme = ","; Literal = None; Line = line; Character = character}
                | '.' -> yield {Type = Period; Lexeme = "."; Literal = None; Line = line; Character = character}
                | '+' -> yield {Type = Plus; Lexeme = "+"; Literal = None; Line = line; Character = character}
                | '*' -> yield {Type = Star; Lexeme = "*"; Literal = None; Line = line; Character = character}
                | '/' -> yield {Type = Slash; Lexeme = "/"; Literal = None; Line = line; Character = character}
                | ';' -> yield {Type = SemiColon; Lexeme = ";"; Literal = None; Line = line; Character = character}
                | ':' -> yield {Type = Colon; Lexeme = ":"; Literal = None; Line = line; Character = character}
                | '=' -> yield {Type = Equal; Lexeme = "="; Literal = None; Line = line; Character = character}
                | '-' -> 
                    yield 
                        match chars.[i + 1] with
                        | '>' ->
                            i <- i + 1
                            character <- character + 1
                            {Type = Arrow; Lexeme = "->"; Literal = None; Line = line; Character = character}
                        | _ -> 
                            {Type = Minus; Lexeme = "-"; Literal = None; Line = line; Character = character}

                //strings
                | '"' ->
                    if chars
                        |> List.skip (i + 1)
                        |> List.contains '"'
                        |> not
                    then
                        yield {Type = Error "String never closed"; Lexeme = string chars.[i]; Literal = None; Line = line; Character = character} 
                        i <- chars.Length - 1
                    else
                        let word = 
                            new string (chars 
                            |> List.skip (i + 1) 
                            |> List.takeWhile (fun x -> x <> '"')
                            |> List.toArray)
                            

                        yield {Type = String; Lexeme = word; Literal = Some word; Line = line; Character = character}
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
                    | "let" -> yield {Type = Let; Lexeme = word; Literal = None; Line = line; Character = character}
                    | "var" -> yield {Type = Var; Lexeme = word; Literal = None; Line = line; Character = character}
                    | _ ->  yield {Type = Id; Lexeme = word; Literal = None; Line = line; Character = character}

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

                    yield {Type = Number; Lexeme = number; Literal = Some (double number); Line = line; Character = character}

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

                | _ -> yield {Type = Error "Unexpected character"; Lexeme = string chars.[i]; Literal = None; Line = line; Character = character} 

        } |> Seq.toList