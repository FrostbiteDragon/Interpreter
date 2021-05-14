using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class While : IStatement
    {
        public IExpression Condition { get; init; }
        public IStatement Statement { get; init; }

        public While(IExpression condition, IStatement statement)
        {
            Condition = condition;
            Statement = statement;
        }
    }
}
