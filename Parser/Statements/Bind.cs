using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class Bind : Statement
    {
        public string Id { get; set; }
        public Expression Value { get; set; }

        public Bind(string id, Expression value)
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
