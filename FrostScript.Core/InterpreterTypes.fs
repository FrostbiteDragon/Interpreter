namespace FrostScript.Core

type DataType =
| AnyType
| NumberType
| BoolType
| StringType
| FunctionType of Input : DataType * Output : DataType
| VoidType

type Expression =
    { Token : Token
      DataType : DataType
      Type : ExpressionType }

and Paramater =
    { Id : string
      Value : DataType }

and ExpressionType =
| BinaryExpression of Left : Expression * Right : Expression
| LiteralExpression of Value : obj
| IdentifierExpression of Id : string
| ValidationError of Error : string
| BindExpression of Id : string * Value : Expression
| AssignExpression of Id : string * Value : Expression
| FunctionExpression of Paramater : Paramater * Body : Expression
| FrostFunction of Closure : Map<string, Expression> * Call : (obj -> obj)
| CallExpression of Callee : Expression * Argument : Expression
| NativeFunction of Call : (obj -> obj)