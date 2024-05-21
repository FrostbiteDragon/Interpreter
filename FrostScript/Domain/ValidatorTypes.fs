namespace FrostScript

type ValidationError = 
    { Token : Token 
      Error : string }

type ValidatorSegment = Node -> (DataType * bool) idMap -> Expression * (DataType * bool) idMap

type ValidatorFunction = ValidatorSegment -> ValidatorSegment