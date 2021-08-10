using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class YieldNode : INode
    {
        public Token Token { get; }
        public INode Value { get; }

        public YieldNode(Token token, INode expression)
        {
            Token = token;
            Value = expression;
        }

        public static readonly Func<ParseFunc, ParseFunc> yield = next => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.Yield)
                return next(pos, tokens);

            var (node, nodePos) = Expression.expression(pos + 1, tokens);

            return (new YieldNode(tokens[pos], node), nodePos);
        };
    }


}
