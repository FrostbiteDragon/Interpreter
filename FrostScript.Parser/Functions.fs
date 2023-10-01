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
        let (node, tokens) = next tokens
        let mutable node = node
        let mutable tokens = tokens

        while List.isEmpty tokens |> not do
            let (ArgumentNode, newTokens) = next tokens

            let CallNode = CallNode (List.head tokens, node, ArgumentNode)
            tokens <- newTokens
            node <- CallNode

        (node, tokens)

        
    let rec expression : ParserFunction = fun tokens ->
        let stop : ParserFunction = fun tokens ->
            (Stop, tokens)

        stop
        |> grouping
        |> primary
        |> factor
        |> term
        |> block
        |> func
        |> call
        |> binding
        |> assign <| tokens

    and grouping (next : ParserFunction) : ParserFunction = fun tokens ->
        let (body, _) =
            match (tokens |> List.head).Type with
            | ParentheseOpen -> 
                expression (tokens |> skipOrEmpty 1 |> List.takeWhile (fun x -> x.Type <> ParentheseClose))
            | _ -> next tokens
            
        let tokens = tokens |> List.skipWhile (fun x -> x.Type <> ParentheseClose)

        if tokens |> List.isEmpty then failwith "token chunk failed to match"

        let nextToken = tokens |> List.head
        match nextToken.Type with
        | ParentheseClose -> (body, tokens |> skipOrEmpty 1)
        | _ -> (ParserError (nextToken, "Expected ')'"), tokens |> skipOrEmpty 1)

    and block (next : ParserFunction) : ParserFunction = fun tokens ->
        let headToken = tokens |> List.head
        match headToken.Type with
        | Pipe ->
            let updateLast update source = 
                let current = source |> List.last
                source |> List.updateAt (source.Length - 1) (update current)

            let bodyTokens = tokens |> List.takeWhile (fun x -> x.Type <> ReturnPipe)
            let body = 
                bodyTokens
                |> List.skip 1
                |> List.takeWhile (fun x -> x.Type <> ReturnPipe)
                |> List.fold (fun tokens token -> 
                    match token.Type with
                    | SemiColon -> List.append tokens [[]]
                    | _ -> tokens |> updateLast (fun lastTokenList -> List.append lastTokenList [token])
                ) [[]]
                |> List.where(fun x -> not x.IsEmpty)
                |> List.map (fun tokens ->  
                    let (node, _) = expression tokens
                    node
                )

            let (value, tokens) = expression (tokens |> List.skip (bodyTokens.Length + 1))

            (BlockNode(headToken, List.append body [value]), tokens)

        | _ -> next tokens

    and func (next : ParserFunction) : ParserFunction = fun tokens ->
        let parameter tokens =
            if (tokens |> skipOrEmpty 3).IsEmpty then (ParserError (tokens.Head, $"Unexpected end of file"), tokens)
            else
                let idToken = tokens.Head
                let colonToken = (tokens |> skipOrEmpty 1).Head
                let typeToken = (tokens |> skipOrEmpty 2).Head

                if idToken.Type <> Id then (ParserError (idToken, $"Expected a paramater name but was instead given {idToken.Type}"), tokens)
                else if colonToken.Type <> Colon then (ParserError (idToken, $"Expected ':' but instead was given {idToken.Type}"), tokens)
                else 
                    match typeToken.Type with
                    | TypeAnnotation paramaterType -> (ParameterNode (idToken.Lexeme, paramaterType), tokens |> skipOrEmpty 3)
                    | _ -> (ParserError (idToken, $"Expected a type but instead was given {idToken.Type}"), tokens)

        let funToken = tokens.Head
        if (funToken.Type <> Fun) then next tokens
        else
            let (parameter, tokens) = parameter (tokens |> skipOrEmpty 1)
            if tokens.Head.Type <> Arrow then (ParserError(funToken, $"Expected '->' but instead was given {tokens.Head.Type}"), tokens)
            else
                match parameter with
                | ParserError _ -> (parameter, tokens)
                | ParameterNode (id, dataType) ->
                    let (body, tokens) = expression (tokens |> skipOrEmpty 1)
                    (FunctionNode(funToken, (id, dataType), body), tokens)
                | _ -> failwith "parameter funtion should only return a parameter node or an error"