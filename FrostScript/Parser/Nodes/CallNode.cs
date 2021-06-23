using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class CallNode : INode
    {
        public INode Callee { get; init; }
        public INode Argument { get; init; }

        public CallNode(INode callee, INode argument)
        {
            Callee = callee;
            Argument = argument;
        }
    }
}
