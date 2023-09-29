open FrostScript

let frostScript = 
    """
        let x = 
        | let y = 1;
          let z = 2;
          spit y;
          spit " + ";
          spit z;
          spit " = "
        |> y + z;

        print x;
    """

"""
    let singleLineFunction = fun x y -> x + y;

    let multiLineFunction = fun x y ->
    |
        print x;
        print y;
        print x + y;
    |> x + y;

    type Test value1:int value2:bool;
    test with { value1 = 2 };
    test.value1;
    
    
    let x =
    | y = 2;
      let y = | print x + 3 |> hello;
    |> y

    x
    |- add 2
    |- print

""" |> ignore

frostScript
|> FrostScript.execute
|> System.Console.WriteLine
|> ignore