[<AutoOpen>]
module FrostScript.Features.FrostList
    open FrostScript.Domain

    let lexList : LexFunc = fun ctx ->
        match ctx.Characters with
        | [] -> Some (Ok ctx)
        | char :: _ ->
            match char with
            | '[' -> 
                { Type = SquareBracketOpen; Lexeme = "["; Literal = None; Position = ctx.Position }
                |> addToken ctx 1
                
            | ']' -> 
                { Type = SquareBracketClose; Lexeme = "]"; Literal = None; Position = ctx.Position }
                |> addToken ctx 1

            | ',' -> 
                { Type = Comma; Lexeme = ","; Literal = None; Position = ctx.Position }
                |> addToken ctx 1
          
            | _ -> None

    let parseList (expression : ParseFunc) : ParseFunc = fun ctx ->
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
                    Error [(tokens.Head, "Expected ']'")]
        else Ok ctx
    
    let validateList (validate : Node -> Result<ValidationOutput, ErrorList>) : ValidationFunc = fun ctx ->
        match ctx.Node.Type with
        | ListNode nodes -> 
            nodes
            |> List.traverseResult (fun node -> validate node)
            |> Result.bind (fun validationOutputs -> 
                let expressions = validationOutputs |> List.map (fun x -> x.Expression)
                let dataType = expressions.Head.DataType 

                expressions
                |> List.map (fun expression -> 
                    if expression.DataType <> dataType then 
                        Error [(ctx.Node.Token, $"Expected {dataType} but was given {expression.DataType}, All elements of a list must be of the same type. ")]
                    else Ok expression
                )
                |> List.sequenceResult
                |> Result.map (fun expressions -> 
                    { Expression = 
                        { DataType = VoidType 
                          Type = ListExpression (expressions) } 
                      Ids = (validationOutputs |> List.last).Ids }
                )
            ) 
            |> Some

        | _ -> None

    let interpretList (interpret : Expression -> Result<InterpretOutput, ErrorList>) : InterpretFunc = fun ctx ->
        match ctx.Expression.Type with
        | ListExpression values ->
            values 
            |> List.traverseResult (fun x -> interpret x |> Result.map (fun x -> x.Value))
            |> Result.map(fun values ->   
                { Value = values
                  Ids = ctx.Ids }
            )
            |> Some

        | _ -> None