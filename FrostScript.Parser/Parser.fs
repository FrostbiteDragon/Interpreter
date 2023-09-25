namespace FrostScript
open FrostScript.Core

module Parser =
    let parse : Parser = fun tokens ->
        let updateLast update source = 
            let current = source |> List.last
            source |> List.updateAt (source.Length - 1) (update current)

        tokens
        |> List.fold (fun tokens token -> 
            match token.Type with
            | SemiColon -> tokens |> List.append []
            | _ -> tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token])
        ) [[]]
        |> List.map (fun tokens ->  
            let (node, _) = Functions.expression tokens
            node
        )