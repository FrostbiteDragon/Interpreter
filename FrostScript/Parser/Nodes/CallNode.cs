using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class CallNode : INode
    {
        public INode Callee { get; init; }
        public INode Argument { get; init; }

        public CallNode(INode callee, INode argument)
        {
            Callee = callee;
            Argument = argument;
        }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> call = (next) => (pos, tokens) =>
        {
            var (node, newPos) = next(pos, tokens);

            var currentPos = newPos;
            var callee = node;
            while (tokens[currentPos].Type is TokenType.ParentheseOpen)
            {
                if (tokens[currentPos + 1].Type is TokenType.ParentheseClose)
                    return (
                        new CallNode(callee, new LiteralNode(new(TokenType.Void))),
                        currentPos + 2
                    );

                var (argument, argumentPos) = NodeParser.Expression(currentPos + 1, tokens);

                if (tokens[argumentPos].Type is not TokenType.ParentheseClose)
                    throw new ParseException(tokens[argumentPos].Line, tokens[argumentPos].Character, $"Expected ')' but got {tokens[pos].Lexeme}", argumentPos + 1);

                callee = new CallNode(callee, argument);
                currentPos = argumentPos + 1;
            }

            return (callee, currentPos);
        };

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> colonCall = (next) => (pos, tokens) =>
        {
            var (node, newPos) = next(pos, tokens);

            var currentPos = newPos;
            var callee = node;
            while (tokens[currentPos].Type is TokenType.Colon)
            {
                if (tokens[currentPos + 1].Type is TokenType.Colon)
                    return (
                        new CallNode(callee, new LiteralNode(new(TokenType.Void))),
                        currentPos + 2
                    );

                var (argument, argumentPos) = NodeParser.Expression(currentPos + 1, tokens);

                callee = new CallNode(callee, argument);
                currentPos = argumentPos;
            }

            return (callee, currentPos);
        };
    }
}
