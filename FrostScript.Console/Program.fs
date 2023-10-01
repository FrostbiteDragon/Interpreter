open FrostScript

let frostScript = 
    """
        var z = 2;
        let addZ = fun x:num -> fun y:num -> 
        | z = 4;
        |> x + y + z;

        print z;
        print (addZ 2 7);
        print z;
    """

"""
    let singleLineFunction = fun x y -> x + y;

    let multiLineFunction = fun x:int ->
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