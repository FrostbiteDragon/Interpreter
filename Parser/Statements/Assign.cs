using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class Assign : IStatement
    {
        public string Id { get; init; }
        public IExpression Value { get; init; }

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
