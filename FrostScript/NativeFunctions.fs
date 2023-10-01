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
        ("spit", createExpression AnyType VoidType <|
            fun argument ->
                System.Console.Write argument 
        )
        ("read", createExpression VoidType AnyType <|
            fun _ -> System.Console.ReadLine ()
        )
        ("webGET", createExpression StringType StringType <|
            fun argument ->
                let client = new System.Net.Http.HttpClient()
                let result = client.GetAsync(System.Uri(argument :?> string))
                result.Result.Content.ReadAsStringAsync().Result
                |> box
        )
    ]