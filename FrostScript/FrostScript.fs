module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    let execute rawScript =
        rawScript 
        |> Lexer.lex
        |> Parser.parse
        |> Validator.validate
        //|> Interpreter.interpret nativeFunctions

   

   // string -> Result<node list> -> Result<Expression list> -> Result<obj>