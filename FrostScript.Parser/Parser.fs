namespace FrostScript
open FrostScript.Core

module Parser =
    let parse : Parser = fun tokens ->
        let updateLast update source = 
            let current = source |> List.last
            source |> List.updateAt (source.Length - 1) (update current)

        let x = 
            tokens
            |> List.fold (fun tokens token -> 
                match token.Type with
                | SemiColon -> List.append tokens [[]]
                | _ -> tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token])
            ) [[]]

        tokens
        |> List.fold (fun tokens token -> 
            match token.Type with
            | SemiColon -> List.append tokens [[]]
            | _ -> tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token])
        ) [[]]
        |> List.map (fun tokens ->  
            let (node, _) = Functions.expression tokens
            node
        )