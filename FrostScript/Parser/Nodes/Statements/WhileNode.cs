using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class WhileNode : INode
    {
        public Token Token { get; }
        public INode Condition { get; }
        public INode[] Body { get; }

        public WhileNode(Token token, INode condition, INode[] body)
        {
            Token = token;
            Condition = condition;
            Body = body;
        }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> @while = next => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.While)
                return next(pos, tokens);

            var condition = Parser.Expression(pos + 1, tokens);

            if (tokens[condition.pos].Type is not TokenType.BraceOpen)
                throw new ParseException(tokens[condition.pos], $"Expected '{{' but got {tokens[condition.pos]}", condition.pos + 1);

            var currentPos = condition.pos + 1;
            IEnumerable<INode> GetBody()
            {
                for (; tokens[currentPos].Type is not (TokenType.BraceClose or TokenType.Eof);)
                {
                    var result = Parser.Expression(currentPos, tokens);
                    currentPos = result.pos;
                    yield return result.node;
                }

                if (tokens[currentPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens.Last(), $"Expected '}}'. When was not closed", condition.pos + 1);
            }

            return (new WhileNode(tokens[pos], condition.node, GetBody().ToArray()), currentPos + 1);
                
        };
    }
}
