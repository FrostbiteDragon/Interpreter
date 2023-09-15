namespace FrostScript
open FrostScript.Core
type ParserFunction = Token list -> Node * Token list

module Functions =
    let skipOrEmpty count list =
        if list |> List.isEmpty then []
        else list |> List.skip count

    let matchType validTypes tokenType =
        validTypes |> List.contains tokenType

    let primary (next : ParserFunction) : ParserFunction = fun tokens -> 
        match (List.head tokens).Type with
        | Int | String -> (Primary (List.head tokens), tokens |> skipOrEmpty 1)
        | _ -> next tokens

    let binary (next : ParserFunction) : ParserFunction = fun tokens -> 
        let (node, tokens) = next tokens
        let mutable node = node
        let mutable tokens = tokens

        while List.isEmpty tokens |> not && (List.head tokens).Type |> matchType [Plus; Minus] do
            let (rightNode, newTokens) = next (tokens |> skipOrEmpty 1)

            let binaryNode = Binary (List.head tokens, node, rightNode)
            tokens <- newTokens
            node <- binaryNode

        (node, tokens)

    
    let expression : ParserFunction =
        let stop : ParserFunction = fun tokens ->
            (Stop, tokens)

        stop
        |> primary
        |> binary

        