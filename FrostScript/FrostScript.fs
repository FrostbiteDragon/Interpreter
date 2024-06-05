[<AutoOpen>]
module FrostScript.FrostScript
    open FrostScript.Domain.NativeFunctions
    open FrostScript.Domain
    open FrostScript.Features

    let execute =
        let lex (script : string) = 
            let ctx = { Characters = script.ToCharArray () |> Array.toList; Position = { Character = 0; Line = 0; }; Tokens = [] }

            ctx 
            |> choose [ lexLiteral ]
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
                validateLiteral
                validateList validate
            ]

        let interpret expression = expression |> choose [ 
            interpretLiteral
            interpretList
        ]

        lex >> 
        apply (Ok splitTokens) >> 
        bindTraverse parse >>
        bindTraverse validate >>
        Result.map (fun x -> x |> List.map (fun x -> x.Expression)) >>
        bindTraverse interpret >> 
        Result.map (fun x -> x |> List.last)