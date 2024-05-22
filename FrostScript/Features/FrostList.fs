module FrostScript.Features.FrostList
    open FrostScript.Domain
    open FrostScript.Domain.Utilities

    let parse (expression : ParseFunc) : ParseHandler = fun next ctx ->
        let listToken = ctx.Tokens.Head
        if listToken.Type = SquareBracketOpen then
            if (ctx.Tokens |> skipOrEmpty 1).Head.Type = SquareBracketClose then 
                let node = { Token = listToken; Type = ListNode [] }
                Ok { Node = node; Tokens = ctx.Tokens |> skipOrEmpty 2 }
            else
                let mutable tokens = ctx.Tokens
                let nodes = 
                    seq {
                        let result = expression { ctx with Tokens = tokens |> skipOrEmpty 1}
                        match result with 
                        | Ok ctx ->
                            yield ctx.Node
                            tokens <- ctx.Tokens

                            while tokens.IsEmpty |> not && tokens.Head.Type = Comma do
                                let result = expression { ctx with Tokens = tokens |> skipOrEmpty 1}
                                match result with 
                                | Ok ctx ->
                                    yield ctx.Node
                                    tokens <- ctx.Tokens
                                | _ -> ignore ()
                        | _ -> ignore ()
                               
                    } |> Seq.toList
                if tokens.Head.Type = SquareBracketClose then
                    let node = { Token = listToken; Type = ListNode nodes }
                    Ok { Node = node; Tokens = tokens |> skipOrEmpty 1 }
                else
                    Error (tokens.Head, "Expected ']'")
        else next ctx
    
    //let validate (validate : ValidatorSegment) : ValidatorFunction = fun next node ids ->
    //    match node.Type with
    //    | ListNode nodes ->
    //        //this ignores any varriable decloration or mutation implicitely. this should probably be defined as illegal syntax and throw an error
    //        let nodes = 
    //            nodes 
    //            |> List.map (fun x -> validate x ids |> fst) 
    //        let dataType = nodes.Head.DataType
    //        if nodes |> List.exists(fun x -> x.DataType <> dataType) then
    //            let error = { DataType = VoidType; Type = ValidationError (node.Token, "All values of a list must have the same type")}
    //            (error, ids)
    //        else
    //            let listExpression = { DataType = ListType dataType; Type = ListExpression nodes }
    //            (listExpression, ids)
    //    | _ -> next node ids

    let interpret execute ids values = 
        (values |> List.map (fun x -> execute ids x |> fst) |> box, ids)