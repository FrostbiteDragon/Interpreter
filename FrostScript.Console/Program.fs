open FrostScript

let frostScript = 
    """
        let reduce = fun x:num y:num -> 
        | spit x;
          spit " - ";
          spit y;
          spit " = ";
        |> x - y;

        print (reduce 1 2);

    """

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore