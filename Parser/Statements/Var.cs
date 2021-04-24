using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class Var : Statement
    {
        public string Id { get; set; }
        public Expression Value { get; set; }

        public Var(string id, Expression value)
        {
            Id = id;
            Value = value;
        }
    }
}
