using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class BinaryNode : INode
    {
        public INode Left { get; }
        public INode Right { get; }

        public Token Token { get; }

        public BinaryNode(INode left, INode right, Token token)
        {
            Left = left;
            Right = right;
            Token = token;
        }
    }
}
