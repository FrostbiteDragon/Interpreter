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

type ErrorList = (Token * string) List

[<AutoOpen>]
module LexFunctions =
    let splitTokens (tokens : Token list) =
        let appendToLastInState token tokens isBlock =
            let current = tokens |> List.last
            (tokens |> List.updateAt (tokens.Length - 1) (List.append current [token]), isBlock)

        tokens
        |> List.fold (fun state token -> 
            let (tokens, blockDepth) = state
            match token.Type with
            | SemiColon  -> 
                if blockDepth > 0 then appendToLastInState token tokens blockDepth
                else (List.append tokens [[]], blockDepth)
            | BlockOpen       -> appendToLastInState token tokens (blockDepth + 1)
            | BlockReturn -> appendToLastInState token tokens (blockDepth - 1)
            | _          -> appendToLastInState token tokens blockDepth
        ) ([[]], 0)
        |> fst
        |> List.where (fun x -> not x.IsEmpty)
    
    let addToken (ctx : LexContext) charactersUsed token =
            Some (
                Ok { Tokens = (ctx.Tokens @ [token]); 
                     Position = {ctx.Position with Character = ctx.Position.Character + 1} 
                     Characters = ctx.Characters |> skipOrEmpty charactersUsed }
            )