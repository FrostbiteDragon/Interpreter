
module Collection
    open FrostScript
    open FrostScript.Utilities
    
    let private newNode token nodeType = {Token = token; Type = nodeType}

    let private error token message =
        { DataType = VoidType
          Type = ValidationError (token, message) }

    let private expression dataType expression =
        { DataType = dataType
          Type = expression }

    let parse (next : ParserFunction) : ParserFunction = fun tokens ->
        let listToken = tokens.Head
        if listToken.Type = SquareBracketOpen then
            if (tokens |> skipOrEmpty 1).Head.Type = SquareBracketClose then (newNode listToken (ListNode []), tokens |> skipOrEmpty 2)
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
                (newNode listToken (ListNode nodes), tokens)
        else next tokens

    let validate validateNode ids token nodes = 
        let nodes = nodes |> List.map (fun x -> validateNode ids x |> fst)
        let dataType = nodes.Head.DataType
        if nodes |> List.exists(fun x -> x.DataType <> dataType) then (error token "All values of a list must have the same type", ids)
        else (expression (ListType dataType) (ListExpression nodes), ids)

    let validate2 (validate : ValidatorFunction) (next: ValidatorFunction)  : ValidatorFunction = fun (node, ids) ->
        match node.Type with
        | ListNode nodes ->
            // this ignores any varriable decloration or mutation implicitely. this should probably be defined as illegal syntax and throw an error
            let nodes = nodes |> List.map (fun x -> validate (x, ids) |> fst)
            let dataType = nodes.Head.DataType
            if nodes |> List.exists(fun x -> x.DataType <> dataType) then (error node.Token "All values of a list must have the same type", ids)
            else (expression (ListType dataType) (ListExpression nodes), ids)
        | _ -> next (node, ids)

    let interpret execute ids values = 
        (values |> List.map (fun x -> execute ids x |> fst) |> box, ids)