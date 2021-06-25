using FrostScript.Expressions;
using FrostScript.Nodes;
using FrostScript.Statements;
using Frostware.Result;
using static FrostScript.Nodes.BinaryNode;
using static FrostScript.Nodes.FunctionNode;
using static FrostScript.Nodes.CallNode;
using static FrostScript.Nodes.LiteralNode;
using static FrostScript.Nodes.WhenNode;
using static FrostScript.Nodes.AndNode;
using static FrostScript.Nodes.BlockNode;
using FrostScript.DataTypes;

namespace FrostScript
{
    public class NodeParser
    {
        public static Result GenerateNodes(Token[] tokens)
        {
            try
            {
                var ast = Expression(0, tokens).node;

                return Result.Pass(ast);
            }
            catch (ParseException e)
            {
                Reporter.Report(e.Line, e.CharacterPos, e.Message);
                return Result.Fail();
            }
        }

        public static (INode node, int pos) Expression(int pos, Token[] tokens)
        {
            return
                Comparison(
                Binary((tokenType) => tokenType is TokenType.Or)(
                And(
                Binary((tokenType) => tokenType is TokenType.Equal)(
                Binary((tokenType) => tokenType is TokenType.GreaterThen or TokenType.GreaterOrEqual or TokenType.LessThen or TokenType.LessOrEqual)(
                Binary((tokenType) => tokenType is TokenType.Plus or TokenType.Minus)(
                Binary((tokenType) => tokenType is TokenType.Star or TokenType.Slash)(
                Block(
                When(
                Function(
                ColonCall(
                Call(
                Primary(
                Grouping
                )))))))))))))(pos, tokens);

            static (INode node, int pos) Grouping(int pos, Token[] tokens)
            {
                var (node, newPos) = Expression(pos, tokens);
                if (newPos >= tokens.Length || tokens[newPos].Type != TokenType.ParentheseClose)
                {
                    Reporter.Report(tokens[pos].Line, tokens[pos].Character, $"Parenthese not closed");
                    return (node, newPos);
                }
                else return (node, newPos + 1);
            }
        }

        public static (Parameter parameter, int newPos) Parameter(int pos, Token[] tokens)
        {
            var id = tokens[pos].Type switch
            {
                TokenType.Id => tokens[pos].Lexeme,
                _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected Id", pos + 3)
            };

            if (tokens[pos + 1].Type is not TokenType.Colon)
                throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected ':' but got {tokens[pos].Lexeme}", pos + 3);

            IDataType type = tokens[pos + 2].Type switch
            {
                TokenType.IntType => DataType.Int,
                TokenType.DoubleType => DataType.Double,
                TokenType.StringType => DataType.String,
                TokenType.BoolType => DataType.Bool,
                _ => throw new ParseException(tokens[pos + 2].Line, tokens[pos + 2].Character, $"Expected type but got {tokens[pos + 2].Lexeme}", pos + 3)
            };

            return (new Parameter(id, type), pos + 3);
        }
    }
}
