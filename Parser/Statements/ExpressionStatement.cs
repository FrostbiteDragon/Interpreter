using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class ExpressionStatement : Statement
    {
        public Expression Expression { get; init; }

        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

    }
}
