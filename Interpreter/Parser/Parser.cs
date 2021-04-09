using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class Parser
    {
        public static IEnumerable<Node> GenerateAST(IEnumerable<Token> tokens)
        {
            var tokenList = tokens.ToList();

            return new Node[1] { new Node(tokenList[3], new Node(tokenList[1], new(tokenList[0]), new(tokenList[2])), new(tokenList[4])) };
        }
    }
}
