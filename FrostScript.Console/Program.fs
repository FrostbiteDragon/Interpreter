open FrostScript

let frostScript = 
    "let x = 6
     x"

printfn "%O" (FrostScript.execute frostScript)
System.Console.ReadLine() |> ignore
