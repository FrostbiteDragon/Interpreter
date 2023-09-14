using System;
using System.Collections.Generic;
using System.Linq;
using Frostware.Pipe;

namespace FrostScript.Nodes
{
    public class LoopNode : INode
    {
        public Token Token { get; }
        public INode Bind { get; }
        public INode Condition { get; }
        public INode Assign { get; }
        public INode[] Body { get; }

        public LoopNode(Token token, INode bind, INode condition, INode assign, INode[] body)
        {
            Token = token;
            Bind = bind;
            Condition = condition;
            Assign = assign;
            Body = body;
        }

        public static readonly Func<ParseFunc, ParseFunc> loop = next => (pos, tokens) =>
        {
            if (tokens[pos].Type is not (TokenType.For or TokenType.While))
                return next(pos, tokens);

            INode bind = null;
            INode condition = null;
            INode assignment = null;

            var currentPos = pos;

            //bind
            if (tokens[currentPos].Type is TokenType.For)
            {
                var id = tokens[pos + 1].Lexeme;

                if (tokens[currentPos + 2].Type is not TokenType.Assign)
                    throw new ParseException(tokens[currentPos + 2], $"expected '=' but recieved \"{tokens[currentPos + 2].Lexeme}\"", pos + 2);

                var (value, valuePos) = Expression.expression(currentPos + 3, tokens);

                bind = new BindNode(tokens[currentPos], id, value, true);
                currentPos = valuePos;
            }

            //condition
            if (tokens[currentPos].Type is TokenType.While)
            {
                var (conditionNode, conditionPos) = Expression.expression(currentPos + 1, tokens);
                condition = conditionNode;
                currentPos = conditionPos;
            }

            //assignment
            if (tokens[currentPos].Type is TokenType.Increment or TokenType.Decrement)
            {
                var increment = tokens[currentPos].Type is TokenType.Increment;

                var idToken = tokens[currentPos + 1];

                if (tokens[currentPos + 2].Type is not TokenType.By)
                    throw new ParseException(tokens[currentPos + 2], $"Expected \"by\"", currentPos + 1);

                var (value, valuePos) = Expression.expression(currentPos + 3, tokens);

                assignment = new AssignNode(
                    tokens[currentPos],
                    idToken.Lexeme,
                    new BinaryNode(
                        new LiteralNode(idToken),
                        value, new Token(increment ? TokenType.Plus : TokenType.Minus)
                        )
                    );

                currentPos = valuePos;
            }

            if (tokens[currentPos].Type is not TokenType.BraceOpen)
                throw new ParseException(tokens[currentPos], $"Expected '{{'", currentPos + 1);

            currentPos++;
            var body = GetBody().ToArray();
            IEnumerable<INode> GetBody()
            {
                for (; tokens[currentPos].Type is not (TokenType.BraceClose or TokenType.Eof);)
                {
                    var (node, newPos) = Expression.expression
                        .Pipe(YieldNode.yield)
                        (currentPos, tokens);

                    currentPos = newPos;
                    yield return node;
                }

                if (tokens[currentPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens.Last(), $"Expected '}}'. for was not closed", currentPos + 1);
                currentPos++;
            }

            return (new LoopNode(tokens[pos], bind, condition, assignment, body), currentPos);
        };
    }
}
