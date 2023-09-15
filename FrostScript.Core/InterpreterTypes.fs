namespace FrostScript.Core

type DataType =
| NumberType

type Expression =
| BinaryExpression of Token * DataType * Left : Expression * Right : Expression
| PrimaryExpression of Token * DataType
