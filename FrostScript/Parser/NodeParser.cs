using FrostScript.Nodes;
using FrostScript.Statements;
using Frostware.Result;
using static FrostScript.Nodes.BinaryNode;
using static FrostScript.Nodes.FunctionNode;
using static FrostScript.Nodes.CallNode;
using static FrostScript.Nodes.LiteralNode;
using static FrostScript.Nodes.AndNode;
using static FrostScript.Nodes.BlockNode;
using static FrostScript.Nodes.UnaryNode;
using static FrostScript.Nodes.BindNode;
using static FrostScript.Nodes.WhenNode;
using FrostScript.DataTypes;
using System.Collections.Generic;
using System.Linq;
using System;
using Frostware.Pipe;

namespace FrostScript
{
    public class NodeParser
    {
        public static Result GenerateNodes(Token[] tokens)
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
                    var (ast, newPos) = Expression(currentpos, tokens);
                    currentpos = newPos;
                    yield return ast;
                }
            }
        }

       
        public static (INode node, int pos) Expression(int pos, Token[] tokens)
        {
            Func<int, Token[], (INode node, int pos)> grouping = (pos, tokens) =>
            {
                var (node, newPos) = Expression(pos, tokens);
                if (newPos >= tokens.Length || tokens[newPos].Type != TokenType.ParentheseClose)
                {
                    Reporter.Report(tokens[pos].Line, tokens[pos].Character, $"Parenthese not closed");
                    return (node, newPos);
                }
                else return (node, newPos + 1);
            };

            return 
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
                .Pipe(bind)
                (pos, tokens);
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
