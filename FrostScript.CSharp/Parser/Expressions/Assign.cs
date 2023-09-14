using FrostScript.DataTypes;
using FrostScript.Expressions;

namespace FrostScript.Expressions
{
    public class Assign : IExpression
    {
        public string Id { get; init; }
        public IExpression Value { get; init; }

        public IDataType Type => DataType.Void;

        public Assign(string id, IExpression value)
        {
            Id = id;
            Value = value;
        }

        public void Deconstruct(out string id, out IExpression value)
        {
            id = Id;
            value = Value;
        }
    }
}
