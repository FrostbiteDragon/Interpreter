
using FrostScript.DataTypes;

namespace FrostScript.Expressions
{
    public class Literal : IExpression
    {
        public object Value { get; init; }

        public IDataType Type { get; }

        public Literal(IDataType type, object value)
        {
            Type = type;
            Value = value;
        }
    }
}
