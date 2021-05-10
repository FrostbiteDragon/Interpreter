using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Unary : Expression
    {
        public Token Operator { get; init; }
        public Expression Expression { get; init; }

        public Unary(DataType type, Token @operator, Expression expression) : base(type)
        {
            Operator = @operator;
            Expression = expression;
        }
    }
}
