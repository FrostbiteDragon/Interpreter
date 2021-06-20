using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class StatementBlock : IStatement
    {
        public IStatement[] Statements { get; init; }

        public StatementBlock(IStatement[] statements)
        {
            Statements = statements;
        }
    }
}
