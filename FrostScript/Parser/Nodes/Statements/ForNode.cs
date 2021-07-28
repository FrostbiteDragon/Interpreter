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
        public BindNode Bind { get; }
        public (INode value, bool direction)? Crement { get; }
        public INode Condition { get; }
        public INode[] Body { get; }

        public ForNode(Token token, BindNode bind, (INode value, bool direction)? increment, INode condition, INode[] body)
        {
            Token = token;
            Bind = bind;
            Crement = increment;
            Condition = condition;
            Body = body;
        }

        public static readonly Func<ParseFunc, ParseFunc> @for = next => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.For)
                return next(pos, tokens);

            BindNode bind = null;
            var currentPos = pos + 1;
            if (tokens[currentPos].Type is not (TokenType.Increment or TokenType.Decrement or TokenType.While))
            {
                var result = 
                    Parser.error("Expected bind expression", tokens[currentPos], currentPos + 1)
                    .Pipe(BindNode.bind)(currentPos, tokens);

                bind = result.node as BindNode;
                currentPos = result.pos;
            }

            (INode value, bool crement)? crement = null;
            if (tokens[currentPos].Type is TokenType.Increment or TokenType.Decrement)
            {
                var result = Expression.expression(currentPos + 1, tokens);
                crement = (result.node, tokens[currentPos].Type is TokenType.Increment);
                currentPos = result.pos;
            }

            INode condition = null;
            if (tokens[currentPos].Type is TokenType.While)
            {
                var result = Expression.expression(currentPos + 1, tokens);
                condition = result.node;
                currentPos = result.pos;
            }

            if (tokens[currentPos].Type is not TokenType.BraceOpen)
                throw new ParseException(tokens[currentPos], $"Expected '{{' but got {tokens[currentPos]}", currentPos + 1);

            currentPos += 1;
            IEnumerable<INode> GetBody()
            {
                for (; tokens[currentPos].Type is not (TokenType.BraceClose or TokenType.Eof);)
                {
                    var result = Expression.expression(currentPos, tokens);
                    currentPos = result.pos;
                    yield return result.node;
                }

                if (tokens[currentPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens.Last(), $"Expected '}}'. When was not closed", currentPos + 1);
            }

            return (new ForNode(tokens[pos], bind, crement, condition, GetBody().ToArray()), currentPos + 1);
        };
    }
}
