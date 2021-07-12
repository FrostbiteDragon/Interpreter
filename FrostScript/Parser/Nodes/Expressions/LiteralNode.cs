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

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> primary = next => (pos, tokens) =>
        {
            return tokens[pos].Type switch
            {
                TokenType.True or 
                TokenType.False or
                TokenType.Int or 
                TokenType.Double or
                TokenType.String or
                TokenType.Id or
                TokenType.Void => (new LiteralNode(tokens[pos]), pos + 1),
                _ => next(pos, tokens)
            };
        };
    }
}
