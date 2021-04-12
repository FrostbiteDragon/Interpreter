using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public class Node
    {
        public Token Token { get; set; }
        public Node Left { get; set; }
        public Node Right { get; set; }

        public bool IsParentese { get; set; }

        public Node() { }
        public Node(Token token)
        {
            Token = token;
        }
        public Node(Token token, Node left, Node right)
        {
            Token = token;
            Left = left;
            Right = right;
        }
    }
}