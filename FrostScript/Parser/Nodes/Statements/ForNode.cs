using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Frostware.Pipe;

namespace FrostScript.Nodes
{
    public class ForNode : INode
    {
        public Token Token { get; }
        public INode Bind { get; }
        public INode Condition { get; }
        public INode Assign { get; }
        public INode[] Body { get; }

        public ForNode(Token token, INode bind, INode condition, INode assign, INode[] body)
        {
            Token = token;
            Bind = bind;
            Condition = condition;
            Assign = assign;
            Body = body;
        }

        public static readonly Func<ParseFunc, ParseFunc> @for = next => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.For)
                return next(pos, tokens);
           
            var currentPos = pos + 1;
            var nodes = GetForExpressions().ToArray();
            IEnumerable<INode> GetForExpressions()
            {
                for (int i = 0; tokens[currentPos].Type is not TokenType.BraceOpen || i < 3; i++)
                {
                    if (i == 2 && tokens[currentPos].Type is TokenType.BraceOpen)
                    {
                        yield return null;
                        break;
                    }

                    if (tokens[currentPos].Type is TokenType.Comma)
                    {
                        yield return null;
                        currentPos++;
                        continue;
                    }

                    var (node, newPos) = Expression.expression(currentPos, tokens);
                    yield return node;
                    currentPos = newPos;

                    if (i < 2)
                    {
                        if (tokens[currentPos].Type is not TokenType.Comma)
                            throw new ParseException(tokens[currentPos], $"Expected ','", currentPos + 1);
                        currentPos++;
                    }
                }
            }

            if (tokens[currentPos].Type is not TokenType.BraceOpen)
                throw new ParseException(tokens[currentPos], $"Expected '{{'", currentPos + 1);

            currentPos++;
            var body = GetBody().ToArray();
            IEnumerable<INode> GetBody()
            {
                for (; tokens[currentPos].Type is not (TokenType.BraceClose or TokenType.Eof);)
                {
                    var result = Expression.expression(currentPos, tokens);
                    currentPos = result.pos;
                    yield return result.node;
                }

                if (tokens[currentPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens.Last(), $"Expected '}}'. for was not closed", currentPos + 1);
                currentPos++;
            }


            return (new ForNode(tokens[pos], nodes[0], nodes[1], nodes[2], body), currentPos);
        };
    }
}
