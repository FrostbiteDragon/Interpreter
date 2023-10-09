open FrostScript

let frostScript = 
    """
      if 2 == 2 -> print 1;

    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore