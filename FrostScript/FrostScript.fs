module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    let execute rawScript =
        let x =
            rawScript 
            |> Lexer.lex
            |> Parser.parse

        rawScript 
        |> Lexer.lex
        |> Parser.parse
        //|> Validator.validate nativeFunctions
        //|> Interpreter.interpret nativeFunctions

   