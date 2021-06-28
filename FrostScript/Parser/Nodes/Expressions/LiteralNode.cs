using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class LiteralNode : INode
    {
        public Token Token { get; }

        public LiteralNode(Token token)
        {
            Token = token;
        }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> primary = (Grouping) => (pos, tokens) =>
        {
            var isPrimaryType = tokens[pos].Type is
                TokenType.True or TokenType.False or
                TokenType.Int or TokenType.Double or
                TokenType.String or
                TokenType.Id or
                TokenType.Void;

            if (isPrimaryType) return (new LiteralNode(tokens[pos]), pos + 1);
            else if (tokens[pos].Type == TokenType.ParentheseOpen) return Grouping(pos + 1, tokens);
            else throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected an expression. instead got \"{tokens[pos].Lexeme}\"", pos + 1);
        };
    }
}
