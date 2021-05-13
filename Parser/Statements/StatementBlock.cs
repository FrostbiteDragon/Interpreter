using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class StatementBlock : Statement
    {
        public Statement[] Statements { get; init; }

        public StatementBlock(Statement[] statements)
        {
            Statements = statements;
        }
    }
}
