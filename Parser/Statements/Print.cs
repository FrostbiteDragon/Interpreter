using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class Print : IStatement
    {
        public IExpression Expression { get; init; }

        public Print(IExpression expression)
        {
            Expression = expression;
        }
    }
}
