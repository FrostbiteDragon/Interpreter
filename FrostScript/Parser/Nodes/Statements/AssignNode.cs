using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class AssignNode : INode
    {
        public Token Token { get; }
        public string Id { get; }

        public INode Value { get; }

        public AssignNode(Token token, string id, INode value)
        {
            Token = token;
            Id = id;
            Value = value;
        }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> assign = next => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.Id || tokens[pos + 1].Type is not TokenType.Assign)
                return next(pos, tokens);

            var id = tokens[pos].Type switch
            {
                TokenType.Id => tokens[pos].Lexeme,
                _ => throw new ParseException(tokens[pos], $"Expected an identifier, but instead got '{tokens[pos]}'", pos + 1)
            };

            if (tokens[pos + 1].Type is not TokenType.Assign)
                throw new ParseException(tokens[pos], $"Expected '=', but instead got \"{tokens[pos]}\"", pos + 1);

            var (newValue, newPos) = next(pos + 2, tokens);

            return (new AssignNode(tokens[pos + 1], id, newValue), newPos);
        };
    }
}
