namespace FrostScript

type Node = 
| Stop
| LoopNode of Token * Binding : Node option * Condition : Node * Bodies : Node list 
| IfNode of Token * Condition : Node * True : Node * False : Node option
| FunctionNode of Token * Parameter : Paramater * Body : Node
| BlockNode of Token * Body : Node list
| LiteralNode of Token
| BinaryNode of Token * Operator * Left : Node * Right : Node
| BindNode of Token * Id : string * Mutable : bool * Value : Node
| ParserError of Token * Error : string
| AssignNode of Token * Id : string * Value : Node
| CallNode of Token * Callee : Node * Argument : Node
| ObjectNode of Token * Fields: Map<string, Node>
| ObjectAccessorNode of Token * Accessee : Node * Field : Token
| FieldNode of Token
