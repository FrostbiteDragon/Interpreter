namespace FrostScript

type ValidationError = 
    { Token : Token 
      Error : string }

type ValidatorFunction = Node * (DataType * bool) idMap -> Expression * (DataType * bool) idMap