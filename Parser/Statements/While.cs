using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class While : Statement
    {
        public Expression Condition { get; init; }
        public Statement Statement { get; init; }

        public While(Expression condition, Statement statement)
        {
            Condition = condition;
            Statement = statement;
        }
    }
}
