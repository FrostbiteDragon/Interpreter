
namespace FrostScript.Expressions
{
    public class Identifier : Expression
    {
        public string Id { get; init; }

        public Identifier(string id)
        {
            Id = id;
        }
    }
}
