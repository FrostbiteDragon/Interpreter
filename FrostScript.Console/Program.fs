open FrostScript

let frostScript = 
    """
    for var i = 0 while i < 10 do i = i + 1 do
    | 
        spit i;
        spit " : ";
        print i;
    |> i;
    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore