namespace FrostScript
open FrostScript.Core

module FrostScript =
    let createExpression inputType outputType body =
         { DataType = FunctionType(inputType, outputType)
           Type = NativeFunction body }

    let nativeFunctions =
        [ 
            ("print", createExpression AnyType VoidType <|
                fun argument ->
                    System.Console.WriteLine argument 
            )

            ("read", createExpression VoidType AnyType <|
                fun _ -> System.Console.ReadLine ()
            )
        ]

    let execute rawScript = 
        rawScript 
        |> Lexer.lex
        |> Parser.parse 
        |> Validator.validate nativeFunctions
        |> Interpreter.interpret nativeFunctions