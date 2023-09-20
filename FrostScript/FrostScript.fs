namespace FrostScript
open FrostScript.Core

module FrostScript =
    let nativeFunctions =
        [ ("print", 
            { DataType = FunctionType(AnyType, VoidType)
              Type = NativeFunction (fun argument -> 
                printf "%A" argument; box ()) 
        })]

    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate nativeFunctions
        |> Interpreter.interpret nativeFunctions