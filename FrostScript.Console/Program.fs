open FrostScript

let frostScript = 
    """
      true or false and true
    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore