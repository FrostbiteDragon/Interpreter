module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    let execute rawScript =
        rawScript 
        |> Lexer.lex
        |> Parser.parse
        //|> Validator.validate nativeFunctions
        //|> Interpreter.interpret nativeFunctions

   