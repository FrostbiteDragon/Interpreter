using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class ExpressionStatement : IStatement, IExpression
    {
        public DataType Type => Expression.Type;
        public IExpression Expression { get; init; }

        public ExpressionStatement(IExpression expression)
        {
            Expression = expression;
        }

    }
}
