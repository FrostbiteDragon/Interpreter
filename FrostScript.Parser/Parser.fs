namespace FrostScript
open FrostScript.Core

module Parser =
    let parse : Parser = fun tokens ->
        let split tokens =
            seq {
                let mutable tokenChunk = []
                for token in tokens do
                    if token.Type <> SemiColon then
                        tokenChunk <- List.append tokenChunk [token]
                    else
                        yield tokenChunk
                        tokenChunk <- []
            } |> Seq.toList

        split tokens
        |> List.map (fun tokens ->  
            let (node, _) = Functions.expression tokens
            node
        )