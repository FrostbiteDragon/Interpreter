namespace FrostScript
open FrostScript.Core

module FrostScript =
    let emptyToken = 
        { Type = Id
          Lexeme = ""
          Line = 0
          Character = 0
          Literal = None }

    let nativeFunctions =
        [ ("print", 
            { Token = emptyToken
              DataType = FunctionType(AnyType, VoidType)
              Type = NativeFunction (fun argument -> printf "%A" argument; box ()) })]

    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate nativeFunctions
        |> Interpreter.interpret nativeFunctions