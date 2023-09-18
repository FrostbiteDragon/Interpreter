namespace FrostScript

module FrostScript =
    let nativeFunctions = 

    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate
        |> Interpreter.interpret