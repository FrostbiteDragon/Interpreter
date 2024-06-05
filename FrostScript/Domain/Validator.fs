namespace FrostScript.Domain

type ValidationError = 
    { Token : Token 
      Error : string }

type ValidationContext = 
    { Ids : (DataType * bool) IdMap
      Node : Node }

type ValidationOutput = InterpretContext

type ValidationFunc = ValidationContext -> Result<ValidationOutput, ErrorList> option
