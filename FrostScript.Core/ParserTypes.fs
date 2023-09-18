namespace FrostScript.Core

type Node = 
| Stop
| LiteralNode of Token
| BinaryNode of Token * Left : Node * Right : Node
| BindNode of Token * Id : string * Mutable : bool * Value : Node
| ParserError of Token * Error : string
| AssignNode of Token * Id : string * Value : Node
