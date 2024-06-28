namespace FrostScript.Features
open FrostScript.Domain

type ErrorList = (Token * string) List

type LexContext = 
    { Characters : char list
      Position : Position
      Tokens : Token list }

type ParseContext = 
    { Node : Node
      Tokens : Token list }

type ValidationContext = 
    { Ids : (DataType * bool) IdMap
      Node : Node }

type ValidationOutput = 
    { Ids : (DataType * bool) IdMap
      Expression : Expression }

type InterpretContext = ValidationOutput

type InterpretOutput = 
    { Ids : (DataType * bool) IdMap
      Value : obj }

type Feature =
    { Lexer : LexContext -> Result<LexContext, ErrorList> option
      Parser : (ParseContext -> Result<ParseContext, ErrorList>) -> (ParseContext -> Result<ParseContext, ErrorList>) -> ParseContext -> Result<ParseContext, ErrorList>
      Validator : (Node -> Result<ValidationOutput, ErrorList>) -> ValidationContext -> Result<ValidationOutput, ErrorList> option
      Interpreter : (Expression -> Result<InterpretOutput, ErrorList>) -> InterpretContext -> Result<InterpretOutput, ErrorList> option }
