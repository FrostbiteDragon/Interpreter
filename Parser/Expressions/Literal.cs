
namespace FrostScript.Expressions
{
    public class Literal : Expression
    {
        public object Value { get; init; }

        public Literal(DataType type, object value) : base(type)
        {
            Value = value;
        }
    }
}
