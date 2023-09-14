using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class UnaryNode : INode
    {
        public Token Token { get; }
        public INode Expression { get; }

        public UnaryNode(Token token, INode expression)
        {
            Token = token;
            Expression = expression;
        }

        public static readonly Func<ParseFunc, ParseFunc> unary = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is TokenType.Minus or TokenType.Plus or TokenType.Not)
            {
                var (node, newPos) = unary(next)(pos + 1, tokens);
                return (new UnaryNode(tokens[pos], node), newPos);
            }
            else return next(pos, tokens);
        };
    }
}
