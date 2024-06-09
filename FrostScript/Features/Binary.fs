[<AutoOpen>]
module FrostScript.Features.Binary
    open FrostScript.Domain
    open Utilities

    let Binary (operators : (string * Operator) list) =
        {
            Lexer = fun ctx ->
                match ctx.Characters with
                | [] -> Ok ctx |> Some
                | char :: _ ->
                    let rec testOperator operators =
                        match operators with
                        | [] -> None
                        | (lexeme, operator) :: tail ->
                            if char.ToString() <> lexeme then testOperator tail
                            else
                                { Type = Operator operator; Lexeme = lexeme; Literal = None; Position = ctx.Position }
                                |> addToken ctx 1

                    testOperator operators

            Parser = fun next ctx -> 
                next ctx
                |> Result.bind (fun leftCtx ->
                    match leftCtx.Tokens with
                    | [] -> Ok leftCtx
                    | head :: tail ->
                        let operator = operators |> List.tryFind (fun (lexeme, _) -> lexeme = head.Lexeme)
                        if operator.IsSome then
                            next { leftCtx with Tokens = tail }
                            |> Result.map (fun rightCtx -> 
                                { rightCtx with Node = { Type = BinaryNode (operator.Value |> snd, leftCtx.Node, rightCtx.Node); Token = head } }
                            )
                        else Ok leftCtx
                )


                //ctx
                //|> Result.map (fun ctx ->
                //    if ctx.Tokens = [] then ctx
                //    else 
                //        let mutable node = ctx.Node
                //        let mutable tokens = ctx.Tokens
                //        let mutable keepLooping = true
                //        while keepLooping && List.isEmpty tokens |> not do
                //            match tokens.Head.Type with
                //            | Operator operator ->
                //                if operators |> List.contains operator then
                //                    let ctx = parse { ctx with Tokens = tokens |> skipOrEmpty 1 }

                //                    let binaryNode = ctx |> Result.map (fun ctx -> { Node = (BinaryNode (operator, node, rightNode)) tokens.Head )
                //                    tokens <- newTokens
                //                    node <- binaryNode
                //                else keepLooping <- false
                //            | _ -> keepLooping <- false
                //        {Node = node; Tokens = tokens}
                //)
                //|> Some


            Validator = fun validate ctx ->
               failwith "not implemented"
                    

            Interpreter = fun interpret ctx -> 
                failwith "not implemented"
        }