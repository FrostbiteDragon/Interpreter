
using FrostScript.DataTypes;

namespace FrostScript.Expressions
{
    public class Identifier : IExpression
    {
        public string Id { get; init; }
        public IDataType Type { get; }

        public Identifier(IDataType type, string id)
        {
            Id = id;
            Type = type;
        }
    }
}
