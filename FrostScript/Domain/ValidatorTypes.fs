namespace FrostScript

type ValidationError = 
    { Token : Token 
      Error : string }

type ValidatorFunction = Node -> Expression