[<AutoOpen>]
module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute =
        let lex (script : string) = 
            let ctx = { Characters = script.ToCharArray () |> Array.toList; Position = { Character = 0; Line = 0; }; Tokens = [] }

            let rec getTokens (ctx : LexContext) =
                if ctx.Characters = [] then
                    Ok ctx
                else
                    let whiteSpace = fun ctx ->
                        match ctx.Characters with
                        | [] -> Some (Ok ctx)
                        | char :: tail ->
                            match char with
                            | ' ' | '\t' -> {ctx with Characters = tail; Position = { ctx.Position with Character = ctx.Position.Character + 1} } |> Ok |> Some
                            | _ -> None
                        
                    ctx
                    |> choose [ 
                        lexList
                        lexLiteral
                        whiteSpace
                    ]
                    |> Result.bind getTokens

            ctx 
            |> getTokens
            |> Result.map (fun x -> x.Tokens)

        let parse tokens =
            let rec expression : ParseFunc = fun ctx ->
                let ifNotEmpty onNotEmpty : ParseFunc = fun ctx -> if ctx.Tokens.IsEmpty then Ok ctx else onNotEmpty ctx

                ifNotEmpty (parseList expression) >=>
                ifNotEmpty parseLiteral
                <| ctx

            let ctx = { Tokens = tokens; Node = { Token = tokens.Head; Type = StatementNode } }
            
            expression ctx
            |> Result.map (fun ctx -> ctx.Node)

        let rec validate node = 
            { Node = node; Ids = { Values = [] } } 
            |> choose [
                validateList validate
                validateLiteral
            ]

        let rec interpret expression = 
            { Expression = expression; Ids = { Values = [] } } 
            |> choose [
                interpretList interpret
                interpretLiteral
            ]

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
