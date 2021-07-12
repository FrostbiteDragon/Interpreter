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
        public INode[] Body { get; }

        public BlockNode(INode[] body)
        {
            Body = body;
        }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> block = (next) => (initialPos, tokens) =>
        {
            if (tokens[initialPos].Type is not (TokenType.Pipe or TokenType.ReturnPipe))
                return next(initialPos, tokens);

            var pos = initialPos;
            var statements = GetBlockStatements(initialPos).ToList();
            IEnumerable<INode> GetBlockStatements(int initialPos)
            {
                while (tokens[pos].Type is TokenType.Pipe)
                {
                    var (statement, newPos) = Parser.Expression(pos + 1, tokens);
                    pos = newPos;

                    yield return statement;
                }
            }

            if (tokens[pos].Type is not TokenType.ReturnPipe)
                return (new BlockNode(statements.Append(new LiteralNode(new(TokenType.Void))).ToArray()), pos);
            else
            {
                var (expression, newPos) = Parser.Expression(pos + 1, tokens);
                statements.Add(expression);
                return (new BlockNode(statements.ToArray()), newPos);
            }

        };
    }
}
