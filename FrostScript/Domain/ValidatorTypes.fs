namespace FrostScript.Domain

type ValidationError = 
    { Token : Token 
      Error : string }

type ValidationContext = 
    { Ids : (DataType * bool) IdMap
      Node : Node }

type ValidationOutput =
    { Ids : (DataType * bool) IdMap
      Expression : Expression }

type ValidationResult = Result<Expression, Token * string> option
type ValidationFunc = ValidationContext -> ValidationResult
