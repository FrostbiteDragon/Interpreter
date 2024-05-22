module FrostScript.Features.FrostList
    open FrostScript.Domain
    open FrostScript.Domain.Utilities

    let parse2 (expression : ParserFunc) : ParserHandler = fun next ctx ->
        let listToken = ctx.Tokens.Head
        if listToken.Type = SquareBracketOpen then
            if (ctx.Tokens |> skipOrEmpty 1).Head.Type = SquareBracketClose then 
                let node = { Token = listToken; Type = ListNode [] }
                Some 
                    { Node = node; 
                      Tokens = ctx.Tokens |> skipOrEmpty 2 }
            else
                let mutable tokens = ctx.Tokens
                let nodes = 
                    seq {
                        let result = expression { ctx with Tokens = tokens |> skipOrEmpty 1}
                        match result with 
                        | Some ctx ->
                            yield ctx.Node
                            tokens <- ctx.Tokens

                            while tokens.IsEmpty |> not && tokens.Head.Type = Comma do
                                let result = expression { ctx with Tokens = tokens |> skipOrEmpty 1}
                                match result with 
                                | Some ctx ->
                                    yield ctx.Node
                                    tokens <- ctx.Tokens
                                | _ -> ignore ()
                        | _ -> ignore ()
                               
                    } |> Seq.toList
                let node = { Token = listToken; Type = ListNode nodes }
                Some { Node = node; Tokens = tokens }
        else next ctx
    
    //let parse (expression : ParserSegment) : ParserSegment = fun (node, tokens) ->
    //    let listToken = tokens.Head
    //    if listToken.Type = SquareBracketOpen then
    //        if (tokens |> skipOrEmpty 1).Head.Type = SquareBracketClose then 
    //            let node = { Token = listToken; Type = ListNode [] }
    //            Success (node, tokens |> skipOrEmpty 2)
    //        else
    //            let mutable tokens = tokens
    //            let nodes = 
    //                seq {
    //                    let result = expression (node, tokens |> skipOrEmpty 1)
    //                    match result with 
    //                    | Success (node, newTokens) ->
    //                        yield node
    //                        tokens <- newTokens

    //                        while tokens.IsEmpty |> not && tokens.Head.Type = Comma do
    //                            let result = expression (node, tokens |> skipOrEmpty 1)
    //                            match result with 
    //                            | Success (node, newTokens) ->
    //                                yield node
    //                                tokens <- newTokens
    //                            | _ -> ignore ()
    //                    | _ -> ignore ()
                               
    //                } |> Seq.toList
    //            let node = { Token = listToken; Type = ListNode nodes }
    //            Success (node, tokens)
    //    else NotFound

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