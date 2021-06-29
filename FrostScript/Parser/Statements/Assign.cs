using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class Assign : IStatement
    {
        public string Id { get; init; }
        public IStatement Value { get; init; }

        public Assign(string id, IStatement value)
        {
            Id = id;
            Value = value;
        }

        public void Deconstruct(out string id, out IStatement value)
        {
            id = Id;
            value = Value;
        }
    }
}
