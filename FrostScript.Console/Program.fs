open FrostScript

let frostScript = 
    "4.2 + 3"

printfn "%O" (FrostScript.execute frostScript)
System.Console.ReadLine() |> ignore
