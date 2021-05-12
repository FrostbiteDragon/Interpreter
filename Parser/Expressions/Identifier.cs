
namespace FrostScript.Expressions
{
    public class Identifier : Expression
    {
        public string Id { get; init; }

        public Identifier(DataType type, string id) : base(type)
        {
            Id = id;
        }
    }
}
