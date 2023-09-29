open FrostScript

let frostScript = 
    "
        var y = 0;

        let x =
        | y = 2
        | let y = \"hello\"
        |> y;

        print x;
        print y;
    "

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore