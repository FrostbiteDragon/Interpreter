namespace FrostScript

type Operator =
| Minus | Plus | Devide | Multiply
| Equal | NotEqual | GreaterThen | GreaterOrEqual | LessOrEqual | LessThen | Or | And
| Pipe| AccessorPipe
| ObjectAccessor

type TokenType =
//structural
| SemiColon
| ParentheseOpen
| ParentheseClose
| BraceOpen
| BraceClose
| Comma
| SquareBracketOpen
| SquareBracketClose
| Period
| Arrow
| Assign
| Discard
| BlockOpen
| BlockReturn
| Colon
| LexerError of string
| Operator of Operator
| Not 
//literals
| Number | String | Id | Void | Bool
//Keywords
| If | Else | When 
| Print 
| Var | Let 
| Fun | New
| For | While | Do
| Yield
| Stop
//Types
| TypeAnnotation of DataType


type Token = {
    Type : TokenType
    Lexeme : string
    Literal : obj option
    Line : int
    Character : int
}
