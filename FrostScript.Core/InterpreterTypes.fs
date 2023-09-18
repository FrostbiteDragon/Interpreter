namespace FrostScript.Core

type DataType =
| NumberType
| BoolType
| StringType
| FunctionType
| VoidType

type Expression =
    { Token : Token
      DataType : DataType
      Type : ExpressionType }

and ExpressionType =
| BinaryExpression of Left : Expression * Right : Expression
| LiteralExpression of Value : obj
| IdentifierExpression of Id : string
| ValidationError of Error : string
| BindExpression of Id : string * Value : Expression
| AssignExpression of Id : string * Value : Expression
