using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Unary : IExpression
    {
        public Token Operator { get; init; }
        public IExpression Expression { get; init; }

        public DataType Type => Expression.Type;

        public Unary(Token @operator, IExpression expression)
        {
            Operator = @operator;
            Expression = expression;
        }
    }
}
