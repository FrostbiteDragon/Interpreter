using FrostScript.DataTypes;
using FrostScript.Statements;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript.Expressions
{
    public class ExpressionBlock : IExpression
    {
        public IStatement[] Statements { get; init; }
        public IDataType Type => (Statements.Last() as ExpressionStatement).Type;

        public ExpressionBlock(IEnumerable<IStatement> statements)
        {
            Statements = statements.ToArray();
        }
    }
}
