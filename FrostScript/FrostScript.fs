namespace FrostScript
open FrostScript.Core

module FrostScript =
    let nativeFunctions =
        [ ("print", 
            { Token = 
                { Type = Id
                  Lexeme = ""
                  Line = 0
                  Character = 0
                  Literal = None }
              DataType = FunctionType(AnyType, VoidType)
              Type = NativeFunction (fun argument -> printf "%A" argument; box ()) })]

    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate nativeFunctions
        |> Interpreter.interpret nativeFunctions