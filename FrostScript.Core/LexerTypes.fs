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
| Number | String | True | False | Id | Void 
//Keywords
| If | Else | When 
| Print 
| Var | Let 
| Fun 
| For | While | Increment | Decrement | By
| Yeild
//Types
//| IntType | DoubleType | StringType | BoolType


type Token = {
    Type : TokenType
    Lexeme : string
    Literal : obj option
    Line : int
    Character : int
}