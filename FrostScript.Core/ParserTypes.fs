namespace FrostScript.Core

type Node = 
| Stop
| PrimaryNode of Token
| BinaryNode of Token * Left : Node * Right : Node
