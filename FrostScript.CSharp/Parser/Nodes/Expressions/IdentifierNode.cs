using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class IdentifierNode : INode
    {
        public Token Token { get; }
        public string Id { get; }

        public IdentifierNode(Token token, string id)
        {
            Token = token;
            Id = id;
        }
    }
}
