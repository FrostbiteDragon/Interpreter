namespace FrostScript
open FrostScript.Core

module Parser =
    let parse : Parser = fun tokens ->
        let rec getNodes tokens nodes =
            let (node, tokens) = Functions.expression tokens
                
            if tokens |> List.isEmpty then 
                List.append nodes [node]
            else 
                getNodes tokens (List.append nodes [node])

        getNodes tokens []