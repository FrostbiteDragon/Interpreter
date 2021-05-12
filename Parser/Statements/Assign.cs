using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class Assign : Statement
    {
        public string Id { get; init; }
        public Expression Value { get; init; }

        public Assign(string id, Expression value)
        {
            Id = id;
            Value = value;
        }

        public void Deconstruct(out string id, out Expression value)
        {
            id = Id;
            value = Value;
        }
    }
}
