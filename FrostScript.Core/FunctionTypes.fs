namespace FrostScript.Core

type Lexer = string -> Token list
type Parser = Token list -> Node list
type Validator = Node list -> Expression list
type Interpreter = Expression list -> obj
