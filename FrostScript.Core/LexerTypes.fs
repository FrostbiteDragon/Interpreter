namespace FrostScript.Core



type TokenType =
//structural
| SemiColon
| ParentheseOpen
| ParentheseClose
| BraceOpen
| BraceClose
| Comma
| Period
| Arrow
| Assign
| Discard
| Pipe
| ClosePipe
| ReturnPipe
| Colon
| LexerError of string
//math operators
| Minus | Plus | Slash | Star
//logical operators
| Equal | NotEqual | GreaterThen | GreaterOrEqual | LessOrEqual | LessThen | Not | Or
//functional oparators
| PipeOp
//literals
| Number | String | Id | Void | Bool
//Keywords
| If | Else | When 
| Print 
| Var | Let 
| Fun 
| For | While | Increment | Decrement | By
| Yield
//Types
| TypeAnnotation of DataType


type Token = {
    Type : TokenType
    Lexeme : string
    Literal : obj option
    Line : int
    Character : int
}