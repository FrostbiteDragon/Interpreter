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

        public static readonly Func<Func<TokenType, bool>, Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>>> binary = (checkTokens) => (next) => (pos, tokens) =>
        {
            var (node, newPos) = next(pos, tokens);

            while (newPos < tokens.Length && checkTokens(tokens[newPos].Type))
            {
                var result = next(newPos + 1, tokens);

                node = new BinaryNode(node, result.node, tokens[newPos]);
                newPos = result.pos;
            }

            return (node, newPos);
        };

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> equality =
            binary((tokenType) => tokenType is TokenType.Equal);

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> or =
           binary((tokenType) => tokenType is TokenType.Or);

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> comparison = 
            binary((tokenType) => tokenType is TokenType.GreaterThen or TokenType.GreaterOrEqual or TokenType.LessThen or TokenType.LessOrEqual);

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> term =
            binary((tokenType) => tokenType is TokenType.Plus or TokenType.Minus);

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> factor = 
            binary((tokenType) => tokenType is TokenType.Star or TokenType.Slash);
    }
}
