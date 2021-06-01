
namespace FrostScript.Expressions
{
    public class Identifier : IExpression
    {
        public string Id { get; init; }
        public DataType Type { get; }

        public Identifier(DataType type, string id)
        {
            Id = id;
            Type = type;
        }
    }
}
