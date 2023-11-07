namespace FrostScript

type Expression =
    { DataType : DataType
      Type : ExpressionType }

and ExpressionType =
| LoopExpression of Bind : Expression option * Condition : Expression * Bodies : Expression List
| IfExpression of Condition : Expression * True : Expression * False : Expression option
| BinaryExpression of Opporator : TokenType * Left : Expression * Right : Expression
| BlockExpression of Body : Expression list
| LiteralExpression of Value : obj
| IdentifierExpression of Id : string
| ValidationError of Token * Error : string
| BindExpression of Id : string * Value : Expression
| AssignExpression of Id : string * Value : Expression
| FunctionExpression of Paramater : Paramater * Body : Expression
| FrostFunction of Call : (Expression idMap -> obj -> obj * Expression idMap)
| CallExpression of Callee : Expression * Argument : Expression
| NativeFunction of Call : (obj -> obj)