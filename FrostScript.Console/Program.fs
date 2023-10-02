open FrostScript

let frostScript = 
    """
      "hello" + 2

    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore