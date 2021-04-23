using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class Print : Statement
    {
        public Expression Expression { get; init; }

        public Print(Expression expression)
        {
            Expression = expression;
        }
    }
}
