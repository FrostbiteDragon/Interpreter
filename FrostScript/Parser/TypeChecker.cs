using FrostScript.DataTypes;
using FrostScript.Expressions;
using FrostScript.Nodes;
using FrostScript.Statements;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class TypeChecker
    {
        public static Result ToTypedNode(this INode[] ast, Dictionary<string, IExpression> nativeFunctions)
        {
            try
            {
                var identifiers = nativeFunctions.ToDictionary(x => x.Key, x => x.Value.Type);

                var typedAst = ast.Select(x => Convert(x, identifiers)).ToArray();

                return Result.Pass(typedAst);
            }
            catch (TypeException e)
            {
                Reporter.Report(e.Line, e.CharacterPos, e.Message);
                return Result.Fail();
            }

            IExpression Convert(INode node, Dictionary<string, IDataType> identifiers)
            {
                return node switch
                {
                    BindNode bindNode => new Func<IExpression>(() => 
                    {
                        var bind = new Bind(bindNode.Id, Convert(bindNode.Value, identifiers));
                        identifiers[bindNode.Id] = bind.Value.Type;

                        return bind;
                    })(),

                    WhenNode ifNode => new Func<IExpression>(() =>
                    {
                        var clauses = ifNode.Clauses.Select(x => (Convert(x.boolExpression, identifiers), Convert(x.resultExpression, identifiers)));

                        var type = clauses.First().Item2.Type;

                        if (clauses.Any(x => x.Item2.Type != type))
                            throw new TypeException(ifNode.Token, $"All branches in an \"when\" must return the same type");

                        if (type is not VoidType && clauses.Last().Item1.Type is not VoidType)
                            throw new TypeException(ifNode.Token, $"None void \"if\" or \"when\" must have a default clause");

                        return new When(clauses.ToArray());
                    })(),

                    AndNode andNode => new And(Convert(andNode.Left, identifiers), Convert(andNode.Right, identifiers)),

                    BinaryNode binaryNode => new Func<IExpression>(() =>
                    {
                        if (binaryNode.Token.Type is TokenType.PipeOp)
                            return Convert(new CallNode(binaryNode.Right, binaryNode.Left), identifiers);

                        var left = Convert(binaryNode.Left, identifiers);
                        var right = Convert(binaryNode.Right, identifiers);

                        var type = binaryNode.Token.Type switch
                        {
                            TokenType.Equal or 
                            TokenType.GreaterOrEqual or
                            TokenType.GreaterThen or
                            TokenType.LessOrEqual or
                            TokenType.Or or
                            TokenType.LessThen => DataType.Int,

                            TokenType.Plus or
                            TokenType.Minus or
                            TokenType.Star or
                            TokenType.Slash => left.Type switch 
                            {
                                IntType => right.Type is IntType or DoubleType ?
                                    right.Type :
                                    throw new TypeException(binaryNode.Token, $"Oporator '{binaryNode.Token.Lexeme}' cannot be used with types int and {right.Type}"),

                                DoubleType => right.Type is IntType or DoubleType ?
                                    DataType.Double :
                                    throw new TypeException(binaryNode.Token, $"Oporator '{binaryNode.Token.Lexeme}' cannot be used with types double and {right.Type}"),

                                _ => throw new TypeException(binaryNode.Token, $"Oporator '{binaryNode.Token.Lexeme}' cannot be used with types {left.Type} and {right.Type}")
                            },

                            _ => throw new NotImplementedException()
                        };

                        return new Binary(type,left, binaryNode.Token, right);
                    })(),

                    BlockNode blockNode => new Func<IExpression>(() =>
                    {
                        Dictionary<string, IDataType> blockIdentifiers = new(identifiers);

                        var expressions = blockNode.Body.Select(x => new ExpressionStatement(Convert(x, blockIdentifiers)));

                        return new ExpressionBlock(expressions);
                    })(),

                    FunctionNode functionNode => new Func<IExpression>(() => 
                    {
                        Dictionary<string, IDataType> blockIdentifiers = new(identifiers) 
                        {
                            {functionNode.Parameter.Id, functionNode.Parameter.Type}
                        };

                        var body = Convert(functionNode.Body, blockIdentifiers);

                        return new Function(functionNode.Parameter, body, DataType.Function(functionNode.Parameter.Type, body.Type));
                    })(),

                    UnaryNode unaryNode => new Func<IExpression>(() =>
                    {
                        var tokenType = unaryNode.Token.Type;
                        var expression = Convert(unaryNode.Expression, identifiers);

                        if (tokenType is TokenType.Minus or TokenType.Plus && expression.Type is not (DoubleType or IntType))
                            throw new TypeException(unaryNode.Token, $"Oporator '{unaryNode.Token.Lexeme}' cannot be used with type {expression.Type}");
                        else if (tokenType is TokenType.Not && expression.Type is not BoolType)
                            throw new TypeException(unaryNode.Token, $"Oporator '{unaryNode.Token.Lexeme}' cannot be used with type {expression.Type}");

                        return new Unary(unaryNode.Token, Convert(unaryNode.Expression, identifiers));
                    })(),

                    CallNode callNode => new Func<IExpression>(() =>
                    {
                        var callee = Convert(callNode.Callee, identifiers);
                        var argument = Convert(callNode.Argument, identifiers);

                        if (callee.Type is FunctionType func)
                        {
                            if (func.ParameterType is not AnyType && func.ParameterType != argument.Type)
                                throw new TypeException(0, 0, $"Function expected an argument of type {func.ParameterType} but instead was given {argument.Type}");

                            return new Call(callee, argument, func.Result);
                        }
                        else throw new TypeException(0,0, $"Type {callee.Type} is not callable");

                    })(),

                    LiteralNode literalNode => literalNode.Token.Type switch 
                    {
                        TokenType.True => new Literal(DataType.Bool, true),
                        TokenType.False => new Literal(DataType.Bool, false),
                        TokenType.Int => new Literal(DataType.Int, literalNode.Token.Literal),
                        TokenType.Double => new Literal(DataType.Double, literalNode.Token.Literal),
                        TokenType.Void => new Literal(DataType.Void, null),
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
