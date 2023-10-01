open FrostScript

let frostScript = 
    """
      webGET "https://api.publicapis.org/entries"

    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore