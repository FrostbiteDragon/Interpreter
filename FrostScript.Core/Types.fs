namespace FrostScript.Core

type Token = {
    token : string
}

type Node = 
| Base

type Expression =
| ExpressionStatement

type Lexer = string -> Token list
type Parser = Token list -> Node list
type Validator = Node list -> Expression list
type Interpreter = Expression list -> obj option
