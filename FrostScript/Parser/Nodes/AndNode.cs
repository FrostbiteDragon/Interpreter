using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    internal class AndNode : INode
    {
        public INode Left { get; }
        public INode Right { get; }

        public AndNode(INode left, INode right)
        {
            Left = left;
            Right = right;
        }
    }
}
