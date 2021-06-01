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
        public IStatement[] Body { get; init; }

        public While(IExpression condition, IEnumerable<IStatement> body)
        {
            Condition = condition;
            Body = body.ToArray();
        }
    }
}
