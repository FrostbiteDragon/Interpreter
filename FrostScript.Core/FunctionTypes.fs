namespace FrostScript.Core

type Lexer = string -> Token list
type Parser = Token list -> Node list
type Validator = (string * Expression) seq -> Node list -> Expression list
type Interpreter = (string * Expression) seq -> Expression list -> obj
  