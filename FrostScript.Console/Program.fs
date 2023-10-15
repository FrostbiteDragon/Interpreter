open FrostScript

let frostScript = 
    """
        for var i = 0 while i < 10 do
        | 
            spit i;
            spit " : ";
            i = i + 1;
            print i;
        |> i;
    """

let frostScript2 = 
    """
        //block function doesn't check for nested blocks
        var x = 0;
        |
            print "test";
            | print test 2 |>();
        |>();

        for var i = 0 while i < 10 do i = i + 1 do
        |
        |> i;
    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore