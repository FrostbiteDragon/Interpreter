module FrostScript.NativeFunctions
open FrostScript.Core

let nativeFunctions =
    let createExpression inputType outputType body =
            { DataType = FunctionType(inputType, outputType)
              Type = NativeFunction body }

    [ 
        ("print", createExpression AnyType VoidType <|
            fun argument ->
                System.Console.WriteLine argument 
        )

        ("read", createExpression VoidType AnyType <|
            fun _ -> System.Console.ReadLine ()
        )
    ]