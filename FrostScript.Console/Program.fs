open FrostScript
open System.IO
open System

Environment.GetCommandLineArgs().[1]
|> File.ReadAllText
|> FrostScript.execute
|> function
   | Ok v -> printf "%A" v
   | Error e -> Console.WriteLine e
