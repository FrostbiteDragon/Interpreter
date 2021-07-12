using FrostScript.DataTypes;
using FrostScript.Expressions;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript.Expressions
{
    public class ExpressionBlock : IExpression
    {
        public IExpression[] Body { get; init; }
        public IDataType Type => Body.Last().Type;

        public ExpressionBlock(IEnumerable<IExpression> body)
        {
            Body = body.ToArray();
        }
    }
}
