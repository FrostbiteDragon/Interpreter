using FrostScript.Expressions;
using FrostScript.Nodes;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public static class TypeChecker
    {
        public static Result ToTypedNode(this INode ast, Dictionary<string, IExpression> nativeFunctions)
        {
            try
            {
                var typedAst = Convert(ast);

                return Result.Pass(ast);
            }
            catch (TypeException e)
            {
                Reporter.Report(e.Line, e.CharacterPos, e.Message);
                return Result.Fail();
            }

            IExpression Convert(INode node)
            {
                return node switch
                {
                    BinaryNode binaryNode => new Func<IExpression>(() =>
                    {
                        var left = Convert(binaryNode.Left);
                        var right = Convert(binaryNode.Right);

                        var type = binaryNode.Token.Type switch
                        {
                            TokenType.Equal or 
                            TokenType.GreaterOrEqual or
                            TokenType.GreaterThen or
                            TokenType.LessOrEqual or
                            TokenType.LessThen => DataType.Bool,

                            TokenType.Plus or
                            TokenType.Minus or
                            TokenType.Star or
                            TokenType.Slash => left.Type switch 
                            {
                                DataType.Int => right.Type is DataType.Int or DataType.Double ?
                                    right.Type :
                                    throw new TypeException(binaryNode.Token, $"Oporator '{binaryNode.Token.Lexeme}' cannot be used with types int and {right.Type}"),
                                DataType.Double => right.Type is DataType.Int or DataType.Double ?
                                    DataType.Double :
                                    throw new TypeException(binaryNode.Token, $"Oporator '{binaryNode.Token.Lexeme}' cannot be used with types double and {right.Type}"),
                            },

                            _ => throw new NotImplementedException()
                        };

                        return new Binary(type,left, binaryNode.Token, right);
                    })(),
                    
                    CallNode call => new Call(Convert(call.Callee), Convert(call.Argument)),
                    LiteralNode literal => literal.Token.Type switch 
                    {
                        TokenType.True => new Literal(DataType.Bool, true),
                        TokenType.False => new Literal(DataType.Bool, false),
                        TokenType.Int => new Literal(DataType.Int, literal.Token.Literal),
                        TokenType.Double => new Literal(DataType.Double, literal.Token.Literal),
                        TokenType.Void => new Literal(DataType.Null, literal.Token.Literal),
                        TokenType.String => new Literal(DataType.String, literal.Token.Literal),
                        TokenType.Id => nativeFunctions[literal.Token.Lexeme]
                    }
                };
            }
        }
    }
}
