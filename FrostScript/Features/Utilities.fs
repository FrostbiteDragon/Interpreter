[<AutoOpen>]
module FrostScript.Features.Utilities
    open FrostScript.Domain

    let valueOrUnit =
        function
        | Some value -> value
        | None -> null

    let skipOrEmpty count list =
        if list |> List.isEmpty then []
        else if count > list.Length then []
        else list |> List.skip count

    let addToken (ctx : LexContext) charactersUsed token =
        { Tokens = (ctx.Tokens @ [token]); 
          Position = {ctx.Position with Character = ctx.Position.Character + 1} 
          Characters = ctx.Characters |> skipOrEmpty charactersUsed }
        |> Ok
        |> Some

    let splitTokens (tokens : Token list) =
        let appendToLastInState token tokens isBlock =
            let current = tokens |> List.last
            (tokens |> List.updateAt (tokens.Length - 1) (List.append current [token]), isBlock)

        tokens
        |> List.fold (fun state token -> 
            let (tokens, blockDepth) = state
            match token.Type with
            | SemiColon  -> 
                if blockDepth > 0 then appendToLastInState token tokens blockDepth
                else (List.append tokens [[]], blockDepth)
            | BlockOpen       -> appendToLastInState token tokens (blockDepth + 1)
            | BlockReturn -> appendToLastInState token tokens (blockDepth - 1)
            | _          -> appendToLastInState token tokens blockDepth
        ) ([[]], 0)
        |> fst
        |> List.where (fun x -> not x.IsEmpty)