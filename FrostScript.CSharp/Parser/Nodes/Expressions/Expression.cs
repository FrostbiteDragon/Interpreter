using System;
using Frostware.Pipe;
using static FrostScript.Nodes.BinaryNode;
using static FrostScript.Nodes.FunctionNode;
using static FrostScript.Nodes.CallNode;
using static FrostScript.Nodes.LiteralNode;
using static FrostScript.Nodes.AndNode;
using static FrostScript.Nodes.BlockNode;
using static FrostScript.Nodes.UnaryNode;
using static FrostScript.Nodes.BindNode;
using static FrostScript.Nodes.WhenNode;
using static FrostScript.Nodes.LoopNode;
using static FrostScript.Nodes.AssignNode;

namespace FrostScript.Nodes
{
    public static class Expression
    {
        private static readonly ParseFunc grouping = (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.ParentheseOpen)
                throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected an expression. instead got \"{tokens[pos].Lexeme}\"", pos + 1);

            var (node, newPos) = expression(pos + 1, tokens);
            if (newPos >= tokens.Length || tokens[newPos].Type != TokenType.ParentheseClose)
                throw new ParseException(
                    tokens[newPos].Line,
                    tokens[newPos].Character,
                    $"Expected an expression. instead got \"{tokens[pos].Lexeme}\". Parenthese at {tokens[pos].Line},{tokens[pos].Character} not closed",
                    newPos);

            else return (node, newPos + 1);
        };

        public static readonly ParseFunc expression = 
            grouping
            .Pipe(primary)
            .Pipe(call)
            .Pipe(colonCall)
            .Pipe(pipe)
            .Pipe(unary)
            .Pipe(when)
            .Pipe(@if)
            .Pipe(function)
            .Pipe(block)
            .Pipe(factor)
            .Pipe(term)
            .Pipe(comparison)
            .Pipe(equality)
            .Pipe(and)
            .Pipe(or)
            //statements
            //.Pipe(@while)
            .Pipe(loop)
            .Pipe(assign)
            .Pipe(bind);

        public static readonly ParseFunc none = (pos, _) => (null, pos);
    }
}
