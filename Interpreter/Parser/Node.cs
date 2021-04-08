using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class Node
    {
        public Token Left { get; init; }
        public Token Oporator { get; init; }
        public Token Right { get; init; }
       
        public Node(Token left, Token oporator, Token right)
        {
            Left = left;
            Oporator = oporator;
            Right = right;
        }
    }
}
