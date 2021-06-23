using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class LiteralNode : INode
    {
        public Token Token { get; }

        public LiteralNode(Token token)
        {
            Token = token;
        }
    }
}
