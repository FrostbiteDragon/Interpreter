namespace FrostScript

type NodeType = 
| Stop
| LiteralNode
| LoopNode of Binding : Node option * Condition : Node * Bodies : Node list 
| IfNode of Condition : Node * True : Node * False : Node option
| FunctionNode of Parameter : Paramater * Body : Node
| BlockNode of Body : Node list
| BinaryNode of Operator * Left : Node * Right : Node
| BindNode of Id : string * Mutable : bool * Value : Node
| ParserError of Error : string
| AssignNode of Id : string * Value : Node
| CallNode of Callee : Node * Argument : Node
| ListNode of values : Node list
| ObjectNode of Fields: Map<string, Node>

and Node =
    { Token : Token
      Type : NodeType }
    static member Stop = { Token = { Type = TokenType.Stop; Lexeme = ""; Literal = None; Line = 0; Character = 0 }; Type = Stop }

