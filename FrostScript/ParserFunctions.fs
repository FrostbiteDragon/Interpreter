
module FrostScript.ParserFunctions
    open FrostScript.Domain
    open FrostScript.Domain.Utilities
    open FrostScript.Domain.Railway
    open FrostScript.Features

    let private newNode token nodeType = {Token = token; Type = nodeType}
    let private error token message = {Token = token; Type = ParserError message}
    
    //let lexerError (next : ParserSegment) : ParserSegment = fun tokens -> 
    //    match tokens.Head.Type with
    //    | LexerError message -> (error tokens.Head message, tokens)
    //    | _ -> next tokens

    //let binary validTypes (next : ParserSegment) : ParserSegment = fun tokens -> 
    //    let (node, tokens) = next tokens

    //    if tokens.IsEmpty then (node, tokens)
    //    else 
    //        let mutable node = node
    //        let mutable tokens = tokens
    //        let mutable keepLooping = true
    //        while keepLooping && List.isEmpty tokens |> not do
    //            match tokens.Head.Type with
    //            | Operator operator ->
    //                if validTypes |> List.contains operator then
    //                    let (rightNode, newTokens) = next (tokens |> skipOrEmpty 1)

    //                    let binaryNode = newNode tokens.Head (BinaryNode (operator, node, rightNode))
    //                    tokens <- newTokens
    //                    node <- binaryNode
    //                else keepLooping <- false
    //            | _ -> keepLooping <- false
    //        (node, tokens)

    //let term           = binary [Plus; Minus]
    //let factor         = binary [Multiply; Devide]
    //let equality       = binary [Equal; NotEqual]
    //let comparison     = binary [LessThen; LessOrEqual; GreaterThen; GreaterOrEqual]
    //let andFunction    = binary [And]
    //let orFunction     = binary [Or]
    //let pipe           = binary [Pipe; AccessorPipe]
    //let objectAccessor = binary [ObjectAccessor]

    //let binding (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let bindToken = List.head tokens

    //    let getBind isMutable = 
    //        let idToken = tokens |> skipOrEmpty 1 |> List.tryHead

    //        match (tokens |> skipOrEmpty 2 |> List.tryHead) with
    //        | Some token -> 
    //            if token.Type <> Assign then
    //                (error token "Expected '='", tokens)
    //            else
    //                let (value, tokens) = next (tokens |> skipOrEmpty 3)

    //                match idToken with
    //                | Some token -> (newNode token (BindNode(token.Lexeme, isMutable, value)), tokens)
    //                | None -> (error bindToken "Expected identifier name", tokens)
    //        | None ->  (error bindToken "Expected '='", tokens)

    //    match bindToken.Type with
    //    | Var -> getBind true
    //    | Let -> getBind false
    //    | _ -> next tokens
    
    //let assign (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let idToken = tokens |> List.head
    //    match idToken.Type with
    //    | Id ->
    //        let token = tokens |> skipOrEmpty 1 |> List.tryHead
    //        match token with
    //        | None -> next tokens
    //        | Some token ->
    //            match token.Type with
    //            | Assign -> 
    //                let (value, tokens) = next (tokens |> skipOrEmpty 2) 
    //                (newNode token (AssignNode (idToken.Lexeme, value)), tokens)
    //            | _ -> next tokens
    //    | _ -> next tokens

    //let call (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let (node, tokens) = next tokens
    //    let mutable node = node
    //    let mutable tokens = tokens
    //    let mutable keepLooping = true

    //    while keepLooping && List.isEmpty tokens |> not && tokens.Head.Type <> Period do
    //        let (ArgumentNode, newTokens) = next tokens

    //        if ArgumentNode.Type = Stop then keepLooping <- false
    //        else
    //            let CallNode = newNode tokens.Head (CallNode (node, ArgumentNode))
    //            tokens <- newTokens
    //            node <- CallNode

    //    (node, tokens)

    //let parameterGroup (tokens : Token list) =
    //    let parameter (tokens : Token list) =
    //        if (tokens |> skipOrEmpty 2).IsEmpty then Error $"Unexpected end of file"
    //        else
    //            let idToken = tokens.Head
    //            let typeToken = tokens |> skipOrEmpty 1 |> List.head

    //            if idToken.Type <> Id then Error $"Expected a paramater name but was instead given {idToken.Type}"
    //            else 
    //                match typeToken.Type with
    //                | TypeAnnotation paramaterType -> Ok ({Id = idToken.Lexeme; Value = paramaterType}, tokens |> skipOrEmpty 2)
    //                | _ -> Error $"Expected a type but instead was given {idToken.Type}"

    //    if tokens.Head.Type <> ParentheseOpen then (Error("Expected '('"), tokens)
    //    else 
    //        let mutable tokens = tokens |> skipOrEmpty 1
    //        let mutable parameters = []
    //        let mutable error = None
    //        while tokens <> [] && error = None && tokens.Head.Type <> ParentheseClose do
    //            if tokens.Head.Type <> Id  then error <- Some $"Espected Id but got {tokens.Head.Type}"
    //            else
    //                match error with
    //                | Some _ -> ()
    //                | None ->
    //                    match parameter tokens with
    //                    | Error message -> 
    //                        error <- Some message
    //                    | Ok (parameter, newTokens) ->
    //                        if newTokens.Head.Type <> Comma && newTokens.Head.Type <> ParentheseClose then 
    //                            error <- Some "Expected ','"
    //                            tokens <- newTokens
    //                        else if newTokens.Head.Type <> ParentheseClose then
    //                            tokens <- newTokens |> skipOrEmpty 1
    //                            parameters <- parameters |> List.append [parameter]
    //                        else
    //                            tokens <- newTokens
    //                            parameters <- parameters |> List.append [parameter]

    //        match error with
    //        | Some error -> (Error(error), tokens)
    //        | _ ->
    //            if (tokens.Head.Type <> ParentheseClose) then (Error("Expected ')'"), tokens)
    //            else
    //                (Ok parameters, tokens |> skipOrEmpty 1)

    //let constructor next : ParserSegment = fun tokens ->
    //    let constructorToken = tokens.Head
    //    if constructorToken.Type <> New then next tokens
    //    else
    //        let (parameters, tokens) = parameterGroup (tokens |> skipOrEmpty 1)
    //        match parameters with
    //        | Error message -> (error tokens.Head message, [])
    //        | Ok parameters ->
    //            let object =
    //                let fields =  
    //                    parameters 
    //                    |> List.map (fun x -> (x.Id, newNode { Type = Id; Lexeme = x.Id; Literal = None; Line = 0; Character = 0 } LiteralNode))
    //                    |> Map
    //                newNode constructorToken (ObjectNode fields)
    //            let node = 
    //                parameters
    //                |> skipOrEmpty 1
    //                |> List.fold(fun functionNode parameter -> 
    //                    match functionNode.Type with
    //                    | FunctionNode _ ->
    //                        newNode constructorToken (FunctionNode (parameter, functionNode))
    //                    | _ -> failwith "parameter funtion should only return a parameter node or an error"
    //                ) (newNode constructorToken (FunctionNode(parameters.Head, object)))
    //            (node, tokens)

    let rec expression : ParserFunc = fun ctx ->
        (
            FrostList.parse2 expression >=>
            Literal.parse2
        ) (fun ctx -> Some ctx) ctx

    //and grouping (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let (body, tokens) =
    //        match tokens.Head.Type with
    //        | ParentheseOpen -> 
    //            expression (tokens |> skipOrEmpty 1)
    //        | _ -> next tokens
            
    //    match body.Type with
    //    | Stop -> (body, tokens)
    //    | _ ->
    //        let nextToken = tokens.Head
    //        match nextToken.Type with
    //        | ParentheseClose -> (body, tokens |> skipOrEmpty 1)
    //        | _ -> (error nextToken "Expected ')'", tokens |> skipOrEmpty 1)

    //and block (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let headToken = tokens |> List.head
    //    match headToken.Type with
    //    | BlockOpen ->
    //        let tokens = tokens |> skipOrEmpty 1

    //        let bodyTokens = 
    //            tokens
    //            |> List.rev
    //            |> List.skipWhile (fun x -> x.Type <> BlockReturn)
    //            |> List.skip 1
    //            |> List.rev

    //        let body = 
    //            bodyTokens
    //            |> splitTokens
    //            |> List.map (fun tokens ->  
    //                let (node, _) = expression tokens
    //                node
    //            )

    //        let (value, tokens) = expression (tokens |> List.skip (bodyTokens.Length + 1))

    //        (newNode headToken (BlockNode(List.append body [value])), tokens)

    //    | _ -> next tokens

    //and func (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let funToken = tokens.Head
    //    if funToken.Type <> Fun then next tokens
    //    else
    //        let (parameters, tokens) = parameterGroup (tokens |> skipOrEmpty 1)
    //        match parameters with
    //        | Error message -> (error funToken message, [])
    //        | Ok parameters ->
    //            if tokens.Head.Type <> Arrow then (error funToken $"Expected '->' but instead was given {tokens.Head.Type}", tokens)
    //            else
    //                let (body, tokens) = expression (tokens |> skipOrEmpty 1)
    //                let node = 
    //                    parameters
    //                    |> skipOrEmpty 1
    //                    |> List.fold(fun functionNode parameter -> 
    //                        match functionNode.Type with
    //                        | FunctionNode _ ->
    //                            newNode funToken (FunctionNode (parameter, functionNode))
    //                        | _ -> failwith "parameter funtion should only return a parameter node or an error"
    //                    ) (newNode funToken (FunctionNode(parameters.Head, body)))

    //                (node, tokens)

    //and ifFunction next tokens =
    //    let ifToken = tokens |> List.head

    //    if ifToken.Type <> If then next tokens 
    //    else
    //        let (condition, tokens) = expression (tokens |> skipOrEmpty 1)

    //        if tokens.Head.Type <> Arrow then (error tokens.Head "Expected '->'", tokens) 
    //        else
    //            let (trueNode, tokens) = expression (tokens |> skipOrEmpty 1)

    //            if tokens.IsEmpty || tokens.Head.Type <> Else then (newNode ifToken (IfNode(condition, trueNode, None)), tokens)
    //            else
    //                let (falseNode, tokens) = expression (tokens |> skipOrEmpty 1)
    //                (newNode ifToken (IfNode(condition, trueNode, Some falseNode)), tokens)

    //and loop (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let getBodies (tokens : Token list) = 
    //        if tokens.Head.Type <> Do then (Error "Expected 'do", tokens)
    //        else
    //            let mutable tokens = tokens
    //            let bodies =
    //                seq {
    //                    while tokens.IsEmpty |> not && tokens.Head.Type = Do do
    //                        let (body, newTokens) = expression (tokens |> skipOrEmpty 1)
    //                        tokens <- newTokens
    //                        yield body
    //                } |> Seq.toList
    //            (Ok bodies, tokens)

    //    let loopToken = tokens |> List.head
    //    match loopToken.Type with
    //    | For ->
    //        let bindingTokens = tokens |> skipOrEmpty 1 |> List.takeWhile (fun x -> x.Type <> While)
    //        let (binding, _) = expression bindingTokens
    //        let tokens = tokens |> skipOrEmpty (bindingTokens.Length + 1)

    //        if tokens.Head.Type <> While then (error tokens.Head "Expected 'while'", tokens)
    //        else
    //            let (condition, tokens) = expression (tokens |> skipOrEmpty 1)
    //            let (bodies, tokens) = getBodies tokens

    //            match bodies with
    //            | Error message -> (error tokens.Head message, tokens)
    //            | Ok bodies -> (newNode loopToken (LoopNode(Some binding, condition, bodies)), tokens)
                        
    //    | While ->
    //        let (condition, tokens) = expression (tokens |> skipOrEmpty 1)

    //        let (bodies, tokens) = getBodies tokens

    //        match bodies with
    //        | Error message -> (error tokens.Head message, tokens)
    //        | Ok bodies -> (newNode loopToken (LoopNode(None, condition, bodies)), tokens)
    //    | _ -> next tokens 
                
    //and object (next : ParserSegment) : ParserSegment = fun tokens ->
    //    let objectToken = tokens.Head
    //    match objectToken.Type with
    //    | BraceOpen -> 
    //        let mutable tokens = tokens
    //        let mutable parseError = None 
    //        let fields =
    //            seq {
    //                let mutable breakLoop = false
    //                while not breakLoop do
    //                    match (tokens |> skipOrEmpty 1).Head.Type with
    //                    | Id ->
    //                        let id = (tokens |> List.skip 1).Head.Lexeme
    //                        let (value, newTokens) = expression (tokens |> skipOrEmpty 2)
    //                        tokens <- newTokens
    //                        yield (id, value)
    //                    | _ ->
    //                        parseError <- Some (tokens.Head, "Expected label")
    //                        breakLoop <- true

    //                    if tokens.Head.Type <> Comma then 
    //                        breakLoop <- true
    //            } |> List.ofSeq


    //        match parseError with
    //        | Some (token, message) -> (error token message, tokens)
    //        | None -> 
    //            if tokens.Head.Type <> BraceClose then (error tokens.Head "Expected '}'", tokens)
    //            else (newNode objectToken (ObjectNode(fields |> Map.ofSeq)), tokens |> skipOrEmpty 1)
                
    //    | _ -> next tokens
