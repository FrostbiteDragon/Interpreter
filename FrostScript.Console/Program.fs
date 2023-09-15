open FrostScript

let frostScript = 
    "3 - 4"

printfn "%O" (FrostScript.execute frostScript)
System.Console.ReadLine() |> ignore
