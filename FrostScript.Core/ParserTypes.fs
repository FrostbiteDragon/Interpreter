namespace FrostScript.Core

type Node = 
| Stop
| Primary of Token
| Binary of Token * Left : Node * Right : Node
