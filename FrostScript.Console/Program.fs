open FrostScript

let frostScript = 
    """
        for var i = 1 while i < 10 do i++ do
        |


        |> ();

        
            
        

    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore