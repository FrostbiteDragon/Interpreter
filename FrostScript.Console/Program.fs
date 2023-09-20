open FrostScript

let frostScript = 
    "let x = 7;
     print;"

System.Console.WriteLine (FrostScript.execute frostScript) |> ignore
