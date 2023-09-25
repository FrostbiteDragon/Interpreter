open FrostScript

let frostScript = 
    "print (2 + 2) / 2"

System.Console.WriteLine (FrostScript.execute frostScript) |> ignore
