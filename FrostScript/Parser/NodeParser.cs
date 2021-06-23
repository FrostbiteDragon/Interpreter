using FrostScript.Nodes;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class NodeParser
    {
        public static Result GenerateNodes(Token[] tokens)
        {
            try
            {
                var ast = Term(0, tokens).node;

                return Result.Pass(ast);
            }
            catch (ParseException e)
            {
                Reporter.Report(e.Line, e.CharacterPos, e.Message);
                return Result.Fail();
            }


            (INode node, int pos) Term(int pos, Token[] tokens)
            {
                var (node, newPos) = Factor(pos, tokens);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Plus or TokenType.Minus)
                {
                    var result = Call(newPos + 1, tokens);

                    node = new BinaryNode(node, result.node, tokens[newPos]);
                    newPos = result.pos;
                }

                return (node, newPos);
            }

            (INode node, int pos) Factor(int pos, Token[] tokens)
            {
                var (node, newPos) = Call(pos, tokens);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Star or TokenType.Slash)
                {
                    var result = Call(newPos + 1, tokens);

                    node = new BinaryNode(node, result.node, tokens[newPos]);
                    newPos = result.pos;
                }

                return (node, newPos);
            }

            (INode node, int pos) Call(int pos, Token[] tokens)
            {
                var (node, newPos) = Primary(pos, tokens);

                var currentPos = newPos;
                var callee = node;
                while (tokens[currentPos].Type is TokenType.ParentheseOpen)
                {
                    if (tokens[currentPos + 1].Type is TokenType.ParentheseClose)
                        return (
                            new CallNode(callee, new LiteralNode(new(TokenType.Void))),
                            currentPos + 2
                        );

                    var (argument, argumentPos) = Term(currentPos + 1, tokens);

                    if (tokens[argumentPos].Type is not TokenType.ParentheseClose)
                        throw new ParseException(tokens[argumentPos].Line, tokens[argumentPos].Character, $"Expected ')' but got {tokens[pos].Lexeme}", argumentPos + 1);

                    callee = new CallNode(callee, argument);
                    currentPos = argumentPos + 1;
                }

                return (callee, currentPos);
            }


            (INode node, int pos) Primary(int pos, Token[] tokens)
            {
                var isPrimaryType = tokens[pos].Type is
                    TokenType.True or TokenType.False or
                    TokenType.Int or TokenType.Double or
                    TokenType.String or
                    TokenType.Id or
                    TokenType.Void;

                if (isPrimaryType)
                    return (new LiteralNode(tokens[pos]), pos + 1);
                else throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected an expression. instead got \"{tokens[pos].Lexeme}\"", pos + 1);
            }
        }
    }
}
