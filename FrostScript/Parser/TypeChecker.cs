using FrostScript.Expressions;
using FrostScript.Nodes;
using FrostScript.Statements;
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
                var typedAst = Convert(ast, nativeFunctions.ToDictionary(x => x.Key, x => x.Value.Type));

                return Result.Pass(typedAst);
            }
            catch (TypeException e)
            {
                Reporter.Report(e.Line, e.CharacterPos, e.Message);
                return Result.Fail();
            }

            IExpression Convert(INode node, Dictionary<string, DataType> identifiers)
            {
                return node switch
                {
                    AndNode andNode => new And(Convert(andNode.Left, identifiers), Convert(andNode.Right, identifiers)),

                    BinaryNode binaryNode => new Func<IExpression>(() =>
                    {
                        var left = Convert(binaryNode.Left, identifiers);
                        var right = Convert(binaryNode.Right, identifiers);

                        var type = binaryNode.Token.Type switch
                        {
                            TokenType.Equal or 
                            TokenType.GreaterOrEqual or
                            TokenType.GreaterThen or
                            TokenType.LessOrEqual or
                            TokenType.Or or
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

                                _ => throw new TypeException(binaryNode.Token, $"Oporator '{binaryNode.Token.Lexeme}' cannot be used with types {left.Type} and {right.Type}")
                            },

                            _ => throw new NotImplementedException()
                        };

                        return new Binary(type,left, binaryNode.Token, right);
                    })(),

                    WhenNode whenNode => new Func<IExpression>(() => 
                    {
                        var when = new When(
                            ifExpresion: whenNode.IfExpresion is not null ? Convert(whenNode.IfExpresion, identifiers) : null,
                            resultExpression: Convert(whenNode.ResultExpression, identifiers),
                            elseWhen: whenNode.ElseWhen is not null ? Convert(whenNode.ElseWhen, identifiers) as When : null);


                        static bool VerrifyWhen(When when)
                        {
                            if (when.ElseWhen is not null)
                            {
                                if (when.Type == when.ElseWhen.Type)
                                    return VerrifyWhen(when.ElseWhen);
                                else return false;
                            }
                            else return true;
                        }

                        if (VerrifyWhen(when))
                            return when;
                        else throw new TypeException(0, 0, $"All branches in a when must return the same type");
                    })(),

                    BlockNode blockNode => new Func<IExpression>(() =>
                    {
                        Dictionary<string, DataType> blockIdentifiers = new(identifiers);

                        var expressions = blockNode.Body.Select(x => new ExpressionStatement(Convert(x, blockIdentifiers)));

                        return new ExpressionBlock(expressions);
                    })(),

                    FunctionNode functionNode => new Func<IExpression>(() => 
                    {
                        Dictionary<string, DataType> blockIdentifiers = new(identifiers) 
                        {
                            {functionNode.Parameter.Id, functionNode.Parameter.Type}
                        };
                        return new Function(functionNode.Parameter, Convert(functionNode.Body, blockIdentifiers));
                    })(),

                    UnaryNode unaryNode => new Func<IExpression>(() =>
                    {
                        var tokenType = unaryNode.Token.Type;
                        var expression = Convert(unaryNode.Expression, identifiers);

                        if (tokenType is TokenType.Minus or TokenType.Plus && expression.Type is not (DataType.Double or DataType.Int))
                            throw new TypeException(unaryNode.Token, $"Oporator '{unaryNode.Token.Lexeme}' cannot be used with type {expression.Type}");
                        else if (tokenType is TokenType.Not && expression.Type is not DataType.Bool)
                            throw new TypeException(unaryNode.Token, $"Oporator '{unaryNode.Token.Lexeme}' cannot be used with type {expression.Type}");

                        return new Unary(unaryNode.Token, Convert(unaryNode.Expression, identifiers));
                    })(),

                    CallNode callNode => new Func<IExpression>(() =>
                    {
                        var callee = Convert(callNode.Callee, identifiers);
                        var argument = Convert(callNode.Argument, identifiers);

                        if (callee is ICallableExpression callable)
                        {
                            if (callable.Parameter.Type != argument.Type)
                                throw new TypeException(0, 0, $"Function expected an argument of type {callable.Parameter.Type} but instead was given {argument.Type}");

                            return new Call(callee, argument);
                        }
                        else throw new TypeException(0,0, $"Type {callee.Type} is not callable");

                    })(),

                    LiteralNode literalNode => literalNode.Token.Type switch 
                    {
                        TokenType.True => new Literal(DataType.Bool, true),
                        TokenType.False => new Literal(DataType.Bool, false),
                        TokenType.Int => new Literal(DataType.Int, literalNode.Token.Literal),
                        TokenType.Double => new Literal(DataType.Double, literalNode.Token.Literal),
                        TokenType.Void => new Literal(DataType.Null, literalNode.Token.Literal),
                        TokenType.String => new Literal(DataType.String, literalNode.Token.Literal),
                        TokenType.Id => identifiers.ContainsKey(literalNode.Token.Lexeme) ? 
                            new Identifier(identifiers[literalNode.Token.Lexeme], literalNode.Token.Lexeme) :
                            throw new TypeException(literalNode.Token, $"Identifier {literalNode.Token.Lexeme} is out of scope or does not exist")
                    },

                    _ => throw new NotImplementedException()
                };
            }
        }
    }
}
