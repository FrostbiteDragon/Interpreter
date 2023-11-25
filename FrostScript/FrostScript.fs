namespace FrostScript
open NativeFunctions

module FrostScript =
    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate nativeFunctions
        |> Interpreter.interpret nativeFunctions