namespace FrostScript
open FrostScript.Core

module Validator =
    let validate : Validator = fun nativeFunctions nodes ->
        let rec validateNode (identifiers : (string * DataType) seq) node =
            match node with
            | BinaryNode (token, left, right) -> BinaryExpression (token, NumberType, validateNode identifiers left, validateNode identifiers right)
            | PrimaryNode token -> 
                match token.Type with
                | Id ->
                    if (dict identifiers).ContainsKey token.Lexeme then 
                        PrimaryExpression (token, FunctionType)
                    else
                        ValidationError (token, "Identifier doesn't exists or is out of scope")
                | _ -> PrimaryExpression (token, NumberType)

        let identifiers = nativeFunctions |> Seq.map (fun (x, _) -> (x, VoidType))

        nodes
        |> List.map (fun x -> validateNode identifiers x)
