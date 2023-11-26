namespace FrostScript
open Utilities

type ParserFunction = Token list -> Node * Token list

module ParserFunctions =
    let primary (next : ParserFunction) : ParserFunction = fun tokens -> 
        match (List.head tokens).Type with
        | Number | String | Id | Void | Bool -> (LiteralNode (List.head tokens), tokens |> skipOrEmpty 1)
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

    let term = binary [Operator Plus; Operator Minus]
    let factor = binary [Operator Multiply; Operator Devide]
    let equality = binary [Equal; NotEqual]
    let comparison = binary [LessThen; LessOrEqual; GreaterThen; GreaterOrEqual]
    let andFunction = binary [And]
    let orFunction = binary [Or]

    let objectAccessor (next : ParserFunction) : ParserFunction = fun tokens ->
        let (accessee, tokens) = next tokens
        let mutable accessee = accessee
        let mutable tokens = tokens

        while tokens.IsEmpty |> not && tokens.Head.Type = Period do
            if (tokens |> skipOrEmpty 1).Head.Type <> Id then 
                accessee <- ParserError(tokens.[1], "Expected field name")
            else 
                accessee <- ObjectAccessorNode (tokens.Head, accessee, tokens.[1])
                tokens <- tokens |> skipOrEmpty 2
                    
        (accessee, tokens)
            
    let binding (next : ParserFunction) : ParserFunction = fun tokens ->
        let bindToken = List.head tokens

        let getBind isMutable = 
            let idToken = tokens |> skipOrEmpty 1 |> List.tryHead

            match (tokens |> skipOrEmpty 2 |> List.tryHead) with
            | Some token -> 
                if token.Type <> Assign then
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
                | Assign -> 
                    let (value, tokens) = next (tokens |> skipOrEmpty 2) 
                    (AssignNode (token, idToken.Lexeme, value), tokens)
                | _ -> next tokens
        | _ -> next tokens

    let call (next : ParserFunction) : ParserFunction = fun tokens ->
        let (node, tokens) = next tokens
        let mutable node = node
        let mutable tokens = tokens
        let mutable keepLooping = true

        while keepLooping && List.isEmpty tokens |> not && tokens.Head.Type <> Period do
            let (ArgumentNode, newTokens) = next tokens

            if ArgumentNode = Stop then keepLooping <- false
            else
                let CallNode = CallNode (tokens.Head, node, ArgumentNode)
                tokens <- newTokens
                node <- CallNode

        (node, tokens)

    let parameterGroup (tokens : Token list) =
        let parameter (tokens : Token list) =
            if (tokens |> skipOrEmpty 2).IsEmpty then Error $"Unexpected end of file"
            else
                let idToken = tokens.Head
                let typeToken = tokens |> skipOrEmpty 1 |> List.head

                if idToken.Type <> Id then Error $"Expected a paramater name but was instead given {idToken.Type}"
                else 
                    match typeToken.Type with
                    | TypeAnnotation paramaterType -> Ok ({Id = idToken.Lexeme; Value = paramaterType}, tokens |> skipOrEmpty 2)
                    | _ -> Error $"Expected a type but instead was given {idToken.Type}"

        if (tokens.Head.Type <> ParentheseOpen) then (Error("Expected '('"), tokens)
        else 
            let mutable tokens = tokens |> skipOrEmpty 1
            let mutable parameters = []
            let mutable error = None
            while tokens <> [] && error = None && tokens.Head.Type = Id do
                match error with
                | Some _ -> ()
                | None ->
                    match parameter tokens with
                    | Error message -> 
                        error <- Some message
                    | Ok (parameter, newTokens) ->
                        if newTokens.Head.Type <> Comma && newTokens.Head.Type <> ParentheseClose then 
                            error <- Some "Expected ','"
                            tokens <- newTokens
                        else if newTokens.Head.Type <> ParentheseClose then
                            tokens <- newTokens |> skipOrEmpty 1
                            parameters <- parameters |> List.append [parameter]
                        else
                            tokens <- newTokens
                            parameters <- parameters |> List.append [parameter]

            match error with
            | Some error -> (Error(error), tokens)
            | _ ->
                if (tokens.Head.Type <> ParentheseClose) then (Error("Expected ')'"), tokens)
                else
                    (Ok parameters, tokens |> skipOrEmpty 1)

    let constructor next : ParserFunction = fun tokens ->
        let constructorToken = tokens.Head
        if (constructorToken.Type <> New) then next tokens
        else
            let (parameters, tokens) = parameterGroup (tokens |> skipOrEmpty 1)
            match parameters with
            | Error error -> (ParserError(constructorToken, error), [])
            | Ok parameters ->
                let object =
                    let fields =  
                        parameters 
                        |> List.map (fun x -> (x.Id, LiteralNode { Type = Id; Lexeme = x.Id; Literal = None; Line = 0; Character = 0 }))
                        |> Map
                    ObjectNode(constructorToken, fields)
                let node = 
                    parameters
                    |> skipOrEmpty 1
                    |> List.fold(fun functionNode parameter -> 
                        match functionNode with
                        | FunctionNode _ ->
                            FunctionNode (constructorToken, parameter, functionNode)
                        | _ -> failwith "parameter funtion should only return a parameter node or an error"
                    ) (FunctionNode(constructorToken, parameters.Head, object))
                (node, tokens)

    let stop : ParserFunction = fun tokens ->
        (Stop, tokens)

    let rec expression : ParserFunction = fun tokens ->
        stop
        |> grouping
        |> primary
        |> object
        |> objectAccessor
        |> block
        |> factor
        |> term
        |> comparison
        |> equality
        |> andFunction
        |> orFunction
        |> ifFunction
        |> func
        |> constructor
        |> loop
        |> call
        |> binding
        |> assign <| tokens

    and grouping (next : ParserFunction) : ParserFunction = fun tokens ->
        let (body, tokens) =
            match tokens.Head.Type with
            | ParentheseOpen -> 
                expression (tokens |> skipOrEmpty 1)
            | _ -> next tokens
            
        match body with
        | Stop -> (body, tokens)
        | _ ->
            let nextToken = tokens.Head
            match nextToken.Type with
            | ParentheseClose -> (body, tokens |> skipOrEmpty 1)
            | _ -> (ParserError (nextToken, "Expected ')'"), tokens |> skipOrEmpty 1)

    and block (next : ParserFunction) : ParserFunction = fun tokens ->
        let headToken = tokens |> List.head
        match headToken.Type with
        | Pipe ->
            let tokens = tokens |> skipOrEmpty 1

            let bodyTokens = 
                tokens
                |> List.rev
                |> List.skipWhile (fun x -> x.Type <> ReturnPipe)
                |> List.skip 1
                |> List.rev

            let body = 
                bodyTokens
                |> splitTokens
                |> List.map (fun tokens ->  
                    let (node, _) = expression tokens
                    node
                )

            let (value, tokens) = expression (tokens |> List.skip (bodyTokens.Length + 1))

            (BlockNode(headToken, List.append body [value]), tokens)

        | _ -> next tokens

    and func (next : ParserFunction) : ParserFunction = fun tokens ->
        let funToken = tokens.Head
        if (funToken.Type <> Fun) then next tokens
        else
            let (parameters, tokens) = parameterGroup (tokens |> skipOrEmpty 1)
            match parameters with
            | Error error -> (ParserError(funToken, error), [])
            | Ok parameters ->
                if tokens.Head.Type <> Arrow then (ParserError(funToken, $"Expected '->' but instead was given {tokens.Head.Type}"), tokens)
                else
                    let (body, tokens) = expression (tokens |> skipOrEmpty 1)
                    let node = 
                        parameters
                        |> skipOrEmpty 1
                        |> List.fold(fun functionNode parameter -> 
                            match functionNode with
                            | FunctionNode _ ->
                                FunctionNode (funToken, parameter, functionNode)
                            | _ -> failwith "parameter funtion should only return a parameter node or an error"
                        ) (FunctionNode(funToken, parameters.Head, body))

                    (node, tokens)

    and ifFunction next tokens =
        let ifToken = tokens |> List.head

        if ifToken.Type <> If then next tokens 
        else
            let (condition, tokens) = expression (tokens |> skipOrEmpty 1)

            if tokens.Head.Type <> Arrow then (ParserError (tokens.Head, "Expected '->'"), tokens) 
            else
                let (trueNode, tokens) = expression (tokens |> skipOrEmpty 1)

                if tokens.IsEmpty || tokens.Head.Type <> Else then (IfNode(ifToken, condition, trueNode, None), tokens)
                else
                    let (falseNode, tokens) = expression (tokens |> skipOrEmpty 1)
                    (IfNode(ifToken, condition, trueNode, Some falseNode), tokens)

    and loop (next : ParserFunction) : ParserFunction = fun tokens ->
        let getBodies (tokens : Token list) = 
            if tokens.Head.Type <> Do then (Error "Expected 'do", tokens)
            else
                let mutable tokens = tokens
                let bodies =
                    seq {
                        while tokens.IsEmpty |> not && tokens.Head.Type = Do do
                            let (body, newTokens) = expression (tokens |> skipOrEmpty 1)
                            tokens <- newTokens
                            yield body
                    } |> Seq.toList
                (Ok bodies, tokens)

        let loopToken = tokens |> List.head
        match loopToken.Type with
        | For ->
            let bindingTokens = tokens |> skipOrEmpty 1 |> List.takeWhile (fun x -> x.Type <> While)
            let (binding, _) = expression bindingTokens
            let tokens = tokens |> skipOrEmpty (bindingTokens.Length + 1)

            if tokens.Head.Type <> While then (ParserError(tokens.Head, "Expected 'while'"), tokens)
            else
                let (condition, tokens) = expression (tokens |> skipOrEmpty 1)
                let (bodies, tokens) = getBodies tokens

                match bodies with
                | Error message -> (ParserError(tokens.Head, message), tokens)
                | Ok bodies -> (LoopNode(loopToken, Some binding, condition, bodies), tokens)
                        
        | While ->
            let (condition, tokens) = expression (tokens |> skipOrEmpty 1)

            let (bodies, tokens) = getBodies tokens

            match bodies with
            | Error message -> (ParserError(tokens.Head, message), tokens)
            | Ok bodies -> (LoopNode(loopToken, None, condition, bodies), tokens)
        | _ -> next tokens 
                
    and object (next : ParserFunction) : ParserFunction = fun tokens ->
        let objectToken = tokens.Head
        match objectToken.Type with
        | BraceOpen -> 
            let mutable tokens = tokens
            let mutable error = None 
            let fields =
                seq {
                    let mutable breakLoop = false
                    while not breakLoop do
                        match (tokens |> skipOrEmpty 1).Head.Type with
                        | Id ->
                            let id = (tokens |> List.skip 1).Head.Lexeme
                            let (value, newTokens) = expression (tokens |> skipOrEmpty 2)
                            tokens <- newTokens
                            yield (id, value)
                        | _ ->
                            error <- Some (tokens.Head, "Expected label")
                            breakLoop <- true

                        if tokens.Head.Type <> Comma then 
                            breakLoop <- true
                } |> List.ofSeq


            match error with
            | Some (token, message) -> (ParserError(token, message), tokens)
            | None -> 
                if tokens.Head.Type <> BraceClose then (ParserError(tokens.Head, "Expected '}'"), tokens)
                else (ObjectNode(objectToken, fields |> Map.ofSeq), tokens |> skipOrEmpty 1)
                
        | _ -> next tokens
