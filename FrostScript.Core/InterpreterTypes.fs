namespace FrostScript.Core

type Expression =
    { DataType : DataType
      Type : ExpressionType }

and Paramater =
    { Id : string
      Value : DataType }

and ExpressionType =
| BinaryExpression of opporator : TokenType * Left : Expression * Right : Expression
| BlockExpression of body : Expression list
| LiteralExpression of Value : obj
| IdentifierExpression of Id : string
| ValidationError of Token * Error : string
| BindExpression of Id : string * Value : Expression
| AssignExpression of Id : string * Value : Expression
| FunctionExpression of Paramater : Paramater * Body : Expression
| FrostFunction of Call : (IdentifierMap<Expression> -> obj -> obj * IdentifierMap<Expression>)
| CallExpression of Callee : Expression * Argument : Expression
| NativeFunction of Call : (obj -> obj)