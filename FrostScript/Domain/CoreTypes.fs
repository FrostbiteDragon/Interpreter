namespace FrostScript

type DataType =
| AnyType
| NumberType
| BoolType
| StringType
| FunctionType of Input : DataType * Output : DataType
| VoidType

and Paramater =
    { Id : string
      Value : DataType }