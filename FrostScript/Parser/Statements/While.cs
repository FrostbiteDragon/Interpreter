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
        public IStatement Condition { get; init; }
        public IStatement[] Body { get; init; }

        public While(IStatement condition, IEnumerable<IStatement> body)
        {
            Condition = condition;
            Body = body.ToArray();
        }
    }
}
