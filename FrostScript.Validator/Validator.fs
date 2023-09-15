namespace FrostScript
open FrostScript.Core

module Validator =
    let validate : Validator = fun nodes ->
        let rec validateNode node =
            match node with
            | BinaryNode (token, left, right) -> BinaryExpression (token, NumberType, validateNode left, validateNode right)
            | PrimaryNode token -> PrimaryExpression (token, NumberType)

        nodes
        |> List.map (fun x -> validateNode x)