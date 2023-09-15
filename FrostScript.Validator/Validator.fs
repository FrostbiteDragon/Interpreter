namespace FrostScript
open FrostScript.Core

module Validator =
    let validate : Validator = fun nodes ->
        let rec validateNode node =
            match node with
            | Binary (token, left, right) -> Expression.Binary (token, DataType.Int, validateNode left, validateNode right)
            | Primary token -> Expression.Primary (token, DataType.Int)

        nodes
        |> List.map (fun x -> validateNode x)