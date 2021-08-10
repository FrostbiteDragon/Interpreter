
namespace FrostScript.Expressions
{
    public class Literal : IExpression
    {
        public object Value { get; init; }

        public DataType Type { get; }

        public Literal(DataType type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
