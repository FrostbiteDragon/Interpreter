using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class BlockNode : INode
    {
        public Token Token { get; }
        public INode[] Body { get; }

        public BlockNode(Token token, INode[] body)
        {
            Token = token;
            Body = body;
        }

        public static readonly Func<ParseFunc, ParseFunc> block = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is not (TokenType.Pipe or TokenType.ReturnPipe))
                return next(pos, tokens);

            var currentPos = pos;
            var statements = GetBlockStatements(pos).ToList();
            IEnumerable<INode> GetBlockStatements(int initialPos)
            {
                while (tokens[currentPos].Type is TokenType.Pipe)
                {
                    var (statement, newPos) = Expression.expression(currentPos + 1, tokens);
                    currentPos = newPos;

                    yield return statement;
                }
            }

            if (tokens[currentPos].Type is not TokenType.ReturnPipe)
                return (new BlockNode(tokens[pos], statements.Append(new LiteralNode(new(TokenType.Void))).ToArray()), currentPos);
            else
            {
                var (expression, newPos) = Expression.expression(currentPos + 1, tokens);
                statements.Add(expression);
                return (new BlockNode(tokens[pos], statements.ToArray()), newPos);
            }

        };
    }
}
