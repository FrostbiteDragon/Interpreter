namespace FrostScript
open FrostScript.Core
type ParserFunction = Token list -> Node * Token list

module Functions =
    let skipOrEmpty count list =
        if list |> List.isEmpty then []
        else list |> List.skip count


    let primary (next : ParserFunction) : ParserFunction = fun tokens -> 
        match (List.head tokens).Type with
        | Number | String -> (PrimaryNode (List.head tokens), tokens |> skipOrEmpty 1)
        | _ -> next tokens

    let binary validTypes (next : ParserFunction) : ParserFunction = fun tokens -> 
        let (node, tokens) = next tokens
        let mutable node = node
        let mutable tokens = tokens

        while List.isEmpty tokens |> not && validTypes |> List.contains (List.head tokens).Type do
            let (rightNode, newTokens) = next (tokens |> skipOrEmpty 1)

            let binaryNode = BinaryNode (List.head tokens, node, rightNode)
            tokens <- newTokens
            node <- binaryNode

        (node, tokens)

    let term = binary [Plus; Minus]
    let factor = binary [Star; Slash]
    
    let expression : ParserFunction =
        let stop : ParserFunction = fun tokens ->
            (Stop, tokens)

        stop
        |> primary
        |> factor
        |> term
