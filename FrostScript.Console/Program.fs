open FrostScript
open System.IO

System.Environment.GetCommandLineArgs().[1]
|> File.ReadAllText
|> FrostScript.execute
|> function
   | Ok v -> printf "%A" v
   | Error e -> System.Console.WriteLine e
