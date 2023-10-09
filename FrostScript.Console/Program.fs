open FrostScript

let frostScript = 
    """
      if false -> print 1 
      else print 2;

    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore