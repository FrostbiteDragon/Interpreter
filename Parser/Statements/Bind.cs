using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class Bind : IStatement
    {
        public string Id { get; set; }
        public IExpression Value { get; set; }

        public Bind(string id, IExpression value)
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
