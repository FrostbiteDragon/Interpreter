[<AutoOpen>]
module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute =
        let features = [
            literal
            frostlist
        ]

        let lex (script : string) = 
            let rec getTokens (ctx : LexContext) =
                if ctx.Characters = [] then
                    Ok ctx
                else
                    let whiteSpace = fun ctx ->
                        match ctx.Characters with
                        | [] -> Some (Ok ctx)
                        | char :: tail ->
                            match char with
                            | ' ' | '\t' -> Ok { ctx with Characters = tail; Position = { ctx.Position with Character = ctx.Position.Character + 1 } } |> Some
                            | _ -> None
                        
                    features
                    |> List.map (fun x -> x.Lexer)
                    |> List.append [whiteSpace]
                    |> choose ctx
                    |> Result.bind getTokens

            { Characters = script.ToCharArray () |> Array.toList; Position = { Character = 0; Line = 0; }; Tokens = [] }
            |> getTokens
            |> Result.map (fun x -> x.Tokens)

        let parse tokens =
            let rec getNode (ctx : ParseContext) =
                if ctx.Tokens = [] then
                    Ok ctx
                else
                    features
                    |> List.map (fun x -> x.Parser getNode)
                    |> choose ctx
          
            { Tokens = tokens; Node = { Token = tokens.Head; Type = StatementNode } }
            |> getNode
            |> Result.map (fun ctx -> ctx.Node)

        let rec validate node = 
            features
            |> List.map (fun x -> x.Validator validate)
            |> choose  { Node = node; Ids = { Values = [] } } 

        let rec interpret expression = 
            features
            |> List.map (fun x -> x.Interpreter interpret)
            |> choose { Expression = expression; Ids = { Values = [] } } 

        lex >> 
        apply (Ok splitTokens) >> 
        bindTraverse parse >>
        bindTraverse validate >>
        Result.map (fun validationOutput -> validationOutput |> List.map (fun x -> x.Expression)) >>
        bindTraverse interpret >> 
        Result.map (fun interpretOutput -> (interpretOutput |> List.last).Value) >>
        Result.mapError (fun errors -> 
            errors 
            |> List.map (fun (token, error) -> $"[Line:{token.Position.Line} Character:{token.Position.Character}] {error}")
            |> String.concat System.Environment.NewLine
        )
