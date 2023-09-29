open FrostScript

let frostScript = 
    "
        var y = 0;

        let x =
        | var y = \"hello\"
        |> y;

        print x;
        print y
    "

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore