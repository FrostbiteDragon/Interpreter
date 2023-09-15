namespace FrostScript.Core

type DataType =
| Int

type Expression =
| Binary of Token * DataType * Left : Expression * Right : Expression
| Primary of Token * DataType
