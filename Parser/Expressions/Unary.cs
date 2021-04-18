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
       
        public Unary(Token @operator, Expression expression)
        {
            Operator = @operator;
            Expression = expression;
        }
    }
}
