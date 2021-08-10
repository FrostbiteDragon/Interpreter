using System;

namespace FrostScript.Nodes
{
    public class BinaryNode : INode
    {
        public INode Left { get; }
        public INode Right { get; }

        public Token Token { get; }

        public BinaryNode(INode left, INode right, Token token)
        {
            Left = left;
            Right = right;
            Token = token;
        }

        public static readonly Func<Func<TokenType, bool>, Func<ParseFunc, ParseFunc>> binary = (checkTokens) => (next) => (pos, tokens) =>
        {
            var (node, newPos) = next(pos, tokens);

            while (newPos < tokens.Length && checkTokens(tokens[newPos].Type))
            {
                var result = next(newPos + 1, tokens);

                node = new BinaryNode(node, result.node, tokens[newPos]);
                newPos = result.pos;
            }

            return (node, newPos);
        };

        public static readonly Func<ParseFunc, ParseFunc> equality =
            binary((tokenType) => tokenType is TokenType.Equal);

        public static readonly Func<ParseFunc, ParseFunc> or =
           binary((tokenType) => tokenType is TokenType.Or);

        public static readonly Func<ParseFunc, ParseFunc> comparison = 
            binary((tokenType) => tokenType is TokenType.GreaterThen or TokenType.GreaterOrEqual or TokenType.LessThen or TokenType.LessOrEqual);

        public static readonly Func<ParseFunc, ParseFunc> term =
            binary((tokenType) => tokenType is TokenType.Plus or TokenType.Minus);

        public static readonly Func<ParseFunc, ParseFunc> factor = 
            binary((tokenType) => tokenType is TokenType.Star or TokenType.Slash);

        public static readonly Func<ParseFunc, ParseFunc> pipe =
            binary((tokenType) => tokenType is TokenType.PipeOp);
    }
}
