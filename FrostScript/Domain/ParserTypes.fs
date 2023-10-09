namespace FrostScript

type Node = 
| Stop
| IfNode of Token * Condition : Node * True : Node * False : Node option
| FunctionNode of Token * Parameter : Paramater * Body : Node
| ParameterNode of Name : string * Type : DataType
| BlockNode of Token * Body : Node list
| LiteralNode of Token
| BinaryNode of Token * Left : Node * Right : Node
| BindNode of Token * Id : string * Mutable : bool * Value : Node
| ParserError of Token * Error : string
| AssignNode of Token * Id : string * Value : Node
| CallNode of Token * Callee : Node * Argument : Node
