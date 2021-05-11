using FrostScript.Statements;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript.Expressions
{
    public class ExpressionBlock : Expression
    {
        public Statement[] Statements { get; init; }

        public Expression Value { get; set; }

        public ExpressionBlock(IEnumerable<Statement> statements) : base((statements.Last() as ExpressionStatement).Type)
        {
            Statements = statements.ToArray();
        }
    }
}
