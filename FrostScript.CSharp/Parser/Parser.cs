using FrostScript.Nodes;
using Frostware.Result;
using System.Collections.Generic;
using System.Linq;
using System;
using static FrostScript.Nodes.Expression;

namespace FrostScript
{
    public class Parser
    {
        public static readonly Func<Token[], Result> parse = (tokens) =>
        {
            try
            {
                var ast = GetNodes(0, tokens).ToArray();

                return Result.Pass(ast);
            }
            catch (ParseException e)
            {
                Reporter.Report(e.Line, e.CharacterPos, e.Message);
                return Result.Fail();
            }

            static IEnumerable<INode> GetNodes(int pos, Token[] tokens)
            {
                var currentpos = pos;
                while (tokens[currentpos].Type is not TokenType.Eof)
                {
                    var (ast, newPos) = expression(currentpos, tokens);
                    currentpos = newPos;
                    yield return ast;
                }
            }
        };

        public static readonly Func<string, Token, int, ParseFunc> error = (message, token, pickupPoint) =>
        {
            return (_, _) => throw new ParseException(token, message, pickupPoint);
        };
    }
}
