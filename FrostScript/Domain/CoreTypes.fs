namespace FrostScript

type DataType =
| AnyType
| NumberType
| BoolType
| StringType
| FunctionType of Input : DataType * Output : DataType
| VoidType
| ObjectType of Fields : Map<string, DataType>

and Paramater =
    { Id : string
      Value : DataType }