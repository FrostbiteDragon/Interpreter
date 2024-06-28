[<AutoOpen>]
module FrostScript.Features.Binary
    open FrostScript.Domain
    open Utilities

    let binaryLexer operators ctx = 
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

    let binaryParser operators next _ ctx = 
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

    let binaryValidator func validate ctx =
        match ctx.Node.Type with
        | BinaryNode (operator, left, right) ->
            (validate left, validate right)
            ||> Result.bind2 (func operator ctx)
            |> Some
                    
        | _ -> None
