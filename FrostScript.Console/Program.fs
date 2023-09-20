open FrostScript

let frostScript = 
    "let x = 2;
     print 2 + 2;"

System.Console.WriteLine (FrostScript.execute frostScript) |> ignore
System.Console.ReadLine() |> ignore
