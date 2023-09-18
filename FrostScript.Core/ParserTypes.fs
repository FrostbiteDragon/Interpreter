namespace FrostScript.Core

type Node = 
| Stop
| LiteralNode of Token
| BinaryNode of Token * Left : Node * Right : Node
| BindNode of Token * Mutable : bool * Value : Node
| ParserError of Token * Error : string
