using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public delegate (INode node, int pos) ParseFunc(int pos, Token[] tokens);
}
