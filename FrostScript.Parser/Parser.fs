namespace FrostScript
open FrostScript.Core

module Parser =
    let parse : Parser = fun tokens ->
        let updateLast update source = 
            let current = source |> List.last
            source |> List.updateAt (source.Length - 1) (update current)

        let x = 
            tokens
            |> List.fold (fun state token -> 
                let (tokens, isBlock) = state
                match token.Type with
                | SemiColon -> 
                    if isBlock then (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), isBlock)
                    else (List.append tokens [[]], isBlock)
                | Pipe -> (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), true)
                | ReturnPipe -> (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), false)
                | _ -> (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), isBlock)
            ) ([[]], false)
            |> fst

        tokens
        |> List.fold (fun state token -> 
            let (tokens, isBlock) = state
            match token.Type with
            | SemiColon -> 
                if isBlock then (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), isBlock)
                else (List.append tokens [[]], isBlock)
            | Pipe -> (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), true)
            | ReturnPipe -> (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), false)
            | _ -> (tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token]), isBlock)
        ) ([[]], false)
        |> fst
        |> List.where (fun x -> not x.IsEmpty)
        |> List.map (fun tokens ->
            let (node, _) = Functions.expression tokens
            node
        )