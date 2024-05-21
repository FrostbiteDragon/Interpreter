module FrostScript.Features.List
    open FrostScript.Domain
    open FrostScript.Domain.Utilities
    
    let parse : ParserFunction = fun next tokens ->
        let listToken = tokens.Head
        if listToken.Type = SquareBracketOpen then
            if (tokens |> skipOrEmpty 1).Head.Type = SquareBracketClose then 
                let node = { Token = listToken; Type = ListNode [] }
                (node, tokens |> skipOrEmpty 2)
            else
                let mutable tokens = tokens
                let nodes = 
                    seq {
                        let (node, newTokens) = next (tokens |> skipOrEmpty 1)
                        yield node
                        tokens <- newTokens

                        while tokens.IsEmpty |> not && tokens.Head.Type = Comma do
                            let (node, newTokens) = next (tokens |> skipOrEmpty 1)
                            yield node
                            tokens <- newTokens
                    } |> Seq.toList
                let node = { Token = listToken; Type = ListNode nodes }
                (node, tokens)
        else next tokens

    let validate (validate : ValidatorSegment) : ValidatorFunction = fun next node ids ->
        match node.Type with
        | ListNode nodes ->
            //this ignores any varriable decloration or mutation implicitely. this should probably be defined as illegal syntax and throw an error
            let nodes = 
                nodes 
                |> List.map (fun x -> validate x ids |> fst) 
            let dataType = nodes.Head.DataType
            if nodes |> List.exists(fun x -> x.DataType <> dataType) then
                let error = { DataType = VoidType; Type = ValidationError (node.Token, "All values of a list must have the same type")}
                (error, ids)
            else
                let listExpression = { DataType = ListType dataType; Type = ListExpression nodes }
                (listExpression, ids)
        | _ -> next node ids

    let interpret execute ids values = 
        (values |> List.map (fun x -> execute ids x |> fst) |> box, ids)