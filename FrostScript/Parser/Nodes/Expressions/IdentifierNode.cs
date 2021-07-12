using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class IdentifierNode : INode
    {
        public string Id { get; }

        public IdentifierNode(string id)
        {
            Id = id;
        }
    }
}
