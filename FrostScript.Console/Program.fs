open FrostScript

let frostScript = 
    "6 / 3 + 10"

printfn "%O" (FrostScript.execute frostScript)
System.Console.ReadLine() |> ignore
