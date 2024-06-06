namespace FrostScript.Domain

type Expression =
    { DataType : DataType
      Type : ExpressionType }
    with override this.ToString() =
            match this.Type with
            | LiteralExpression value -> 
                match this.DataType with
                | StringType -> "\"" + (value :?> string) + "\"";
                | _ -> value.ToString()
            | _ -> this.Type.ToString()

and ExpressionType =
| LoopExpression of Bind : Expression option * Condition : Expression * Bodies : Expression List
| IfExpression of Condition : Expression * True : Expression * False : Expression option
| BinaryExpression of Operator : Operator * Left : Expression * Right : Expression
| BlockExpression of Body : Expression list
| LiteralExpression of Value : obj
| IdentifierExpression of Id : string
| FieldExpression of Id : string
| ValidationError of Token * Error : string
| BindExpression of Id : string * Value : Expression
| AssignExpression of Id : string * Value : Expression
| FunctionExpression of Paramater : Paramater * Body : Expression
| CallExpression of Callee : Expression * Argument : Expression
| ListExpression of Values : Expression list
| NativeFunction of Call : (obj -> obj)
| ObjectExpression of fields : Map<string, Expression>

type FrostFunction =
    { Type : DataType
      Call : Expression idMap -> obj -> obj * Expression idMap }
    with override this.ToString() = this.Type.ToString()

type FrostObject =
    { fields : Map<string, obj> }
    with override this.ToString() = 
            let fields = 
                this.fields 
                |> Map.toList 
                |> List.map (fun (id, value) -> $"{id} {value}")
            "{ " + String.concat ", " fields + " }"