namespace FrostScript.Domain

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

type Position =
    { Line : int
      Character : int }

type Token = 
    { Type : TokenType
      Lexeme : string
      Literal : obj option
      Position : Position }

type LexContext = 
    { Characters : char list
      Position : Position
      Tokens : Token list }

type LexResult = Result<LexContext, (Token * string) list> option
type LexFunc = LexContext -> LexResult
type LexHandler = LexFunc -> LexContext -> LexResult