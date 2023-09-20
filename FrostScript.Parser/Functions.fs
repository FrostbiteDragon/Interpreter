namespace FrostScript
open FrostScript.Core
type ParserFunction = Token list -> Node * Token list

module Functions =
    let skipOrEmpty count list =
        if list |> List.isEmpty then []
        else list |> List.skip count

    let primary (next : ParserFunction) : ParserFunction = fun tokens -> 
        match (List.head tokens).Type with
        | Number | String | Id | Void -> (LiteralNode (List.head tokens), tokens |> skipOrEmpty 1)
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

    let binding (next : ParserFunction) : ParserFunction = fun tokens ->
        let bindToken = List.head tokens

        let getBind isMutable = 
            let idToken = tokens |> skipOrEmpty 1 |> List.tryHead

            match (tokens |> skipOrEmpty 2 |> List.tryHead) with
            | Some token -> 
                if token.Type <> Equal then
                    (ParserError (token, "Expected '='"), tokens)
                else
                    let (value, tokens) = next (tokens |> skipOrEmpty 3)

                    match idToken with
                    | Some token -> (BindNode(token, token.Lexeme, isMutable, value), tokens)
                    | None -> (ParserError (bindToken, "Expected identifier name"), tokens)
            | None -> (ParserError (bindToken, "Expected '='"), tokens)

        match bindToken.Type with
        | Var -> getBind true
        | Let -> getBind false
        | _ -> next tokens

    let assign (next : ParserFunction) : ParserFunction = fun tokens ->
        let idToken = tokens |> List.head
        match idToken.Type with
        | Id ->
            let token = tokens |> skipOrEmpty 1 |> List.tryHead
            match token with
            | None -> next tokens
            | Some token ->
                match token.Type with
                | Equal -> 
                    let (value, tokens) = next (tokens |> skipOrEmpty 2) 
                    (AssignNode (token, idToken.Lexeme, value), tokens)
                | _ -> next tokens
        | _ -> next tokens

    let call (next : ParserFunction) : ParserFunction = fun tokens ->
        //let calleeToken = tokens |> List.head
        //let (callee, tokens) = next tokens
        //if tokens.IsEmpty then
        //    (callee, tokens)
        //else
        //    if (tokens |> List.isEmpty |> not) then
        //        let (argument, tokens) = next tokens
        //        (CallNode (calleeToken, callee, argument), tokens)
        //    else (callee, tokens)
        let (node, tokens) = next tokens
        let mutable node = node
        let mutable tokens = tokens

        while List.isEmpty tokens |> not do
            let (ArgumentNode, newTokens) = next tokens

            let CallNode = CallNode (List.head tokens, node, ArgumentNode)
            tokens <- newTokens
            node <- CallNode

        (node, tokens)
        
    let expression : ParserFunction =
        let stop : ParserFunction = fun tokens ->
            (Stop, tokens)

        stop
        |> primary
        |> factor
        |> term
        |> call
        |> binding
        |> assign
