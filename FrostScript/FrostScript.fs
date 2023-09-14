namespace FrostScript

module FrostScript =

    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate 
        |> Interpreter.interpret
        