[<AutoOpen>]
module FrostScript.Features.FrostList
    open FrostScript.Domain
    open Utilities

    let frostlist = {
        Lexer = fun ctx ->
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

        Parser = fun parse ctx ->
            let listToken = ctx.Tokens.Head
            if listToken.Type = SquareBracketOpen then
                if (ctx.Tokens |> skipOrEmpty 1).Head.Type = SquareBracketClose then 
                    let node = { Token = listToken; Type = ListNode [] }
                    Ok { Node = node; Tokens = ctx.Tokens |> skipOrEmpty 2 } |> Some
                else
                    let mutable tokens = ctx.Tokens
                    let nodes = 
                        seq {
                            let result = parse { ctx with Tokens = tokens |> skipOrEmpty 1}
                            match result with 
                            | Ok ctx ->
                                yield ctx.Node
                                tokens <- ctx.Tokens

                                while tokens.IsEmpty |> not && tokens.Head.Type = Comma do
                                    let result = parse { ctx with Tokens = tokens |> skipOrEmpty 1}
                                    match result with 
                                    | Ok ctx ->
                                        yield ctx.Node
                                        tokens <- ctx.Tokens
                                    | _ -> ignore ()
                            | _ -> ignore ()
                               
                        } |> Seq.toList
                    if tokens.Head.Type = SquareBracketClose then
                        let node = { Token = listToken; Type = ListNode nodes }
                        Ok { Node = node; Tokens = tokens |> skipOrEmpty 1 } |> Some
                    else
                        Error [(tokens.Head, "Expected ']'")] |> Some
            else Ok ctx |> Some

        Validator = fun validate ctx ->
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
                            { DataType = ListType dataType
                              Type = ListExpression (expressions) } 
                          Ids = (validationOutputs |> List.last).Ids }
                    )
                ) 
                |> Some

            | _ -> None

        Interpreter = fun interpret ctx ->
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
    }