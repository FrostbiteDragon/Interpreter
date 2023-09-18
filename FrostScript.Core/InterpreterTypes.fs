namespace FrostScript.Core

type DataType =
| NumberType
| FunctionType
| VoidType

type Expression =
| BinaryExpression of Token * DataType * Left : Expression * Right : Expression
| PrimaryExpression of Token * DataType
| ValidationError of Token * Error : string
