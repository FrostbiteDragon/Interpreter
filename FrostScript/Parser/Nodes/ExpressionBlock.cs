using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class ExpressionBlock : INode
    {
        public INode[] Body { get; }

        public ExpressionBlock(INode[] body)
        {
            Body = body;
        }
    }
}
