open FrostScript

let frostScript = 
    "var x = 6
     x"

printfn "%O" (FrostScript.execute frostScript)
System.Console.ReadLine() |> ignore
