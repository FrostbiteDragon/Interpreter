open FrostScript
open System.IO

System.Environment.GetCommandLineArgs().[1]
|> File.ReadAllText
|> FrostScript.execute
|> printf "%A"
