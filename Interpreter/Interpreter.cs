using FrostScript.Expressions;
using FrostScript.Statements;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FrostScript
{
    public static class Interpreter
    {
        public static Stopwatch Stopwatch = new Stopwatch(); 

        public static Result ExecuteProgram(IEnumerable<IStatement> statements, Dictionary<string, IExpression> variables = null)
        {
            Stopwatch.Start();

            if (variables is null)
                variables = new Dictionary<string, IExpression>();

            try
            {
                if (statements.Last() is ExpressionStatement)
                {
                    var result = statements.Last() as ExpressionStatement;

                    foreach (var statement in statements.SkipLast(1))
                        ExecuteStatement(statement, variables);

                    return Result.Pass(ExecuteExpression(result.Expression, variables));
                }
                else
                {
                    foreach (var statement in statements)
                        ExecuteStatement(statement, variables);

                    return Result.Pass();
                }
            }
            catch (InterpretException exception)
            {
                Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
                return Result.Fail();
            }
        }

        public static void ExecuteStatement(IStatement statement, Dictionary<string, IExpression> variables)
        {
            switch (statement)
            {
                case Bind(var id, var value):
                    variables[id] = value;
                    break;

                case Assign(var id, var value):
                    variables[id] = value;
                    break;

                case If ifStmt:
                    ExecuteIf(ifStmt);

                    void ExecuteIf(If ifStmt)
                    {
                        var ifExpression = ifStmt.IfExpresion != null ? ExecuteExpression(ifStmt.IfExpresion, variables) : null;

                        //default clause
                        if (ifExpression is null)
                            ExecuteStatement(ifStmt.ResultStatement, variables);
                        //if clause is true execute
                        else if ((bool)ifExpression)
                            ExecuteStatement(ifStmt.ResultStatement, variables);
                        //else check next clause
                        else
                            ExecuteIf(ifStmt.ElseIf);
                    }
                    break;

                case StatementBlock statementBlock:
                    ExecuteProgram(statementBlock.Statements, variables);
                    break;

                case While @while:
                    while ((bool)ExecuteExpression(@while.Condition, variables))
                    {
                        foreach (var bodyStatement in @while.Body)
                            ExecuteStatement(bodyStatement, variables);
                    }

                    break;

                case For @for:

                    ExecuteStatement(@for.Bind, variables);

                    var bindValue = (double)ExecuteExpression(@for.Bind.Value, variables);

                    while ((bool)ExecuteExpression(@for.EndExpression, variables))
                    {
                        foreach (var bodyStatement in @for.Body)
                            ExecuteStatement(bodyStatement, variables);

                        bindValue += @for.Crement switch
                        {
                            Crement.Increment => 1,
                            Crement.Decrement => -1,
                            _ => throw new ArgumentOutOfRangeException(nameof(@for.Crement))
                        };

                        ExecuteStatement(new Assign(@for.Bind.Id, new Literal(DataType.Numeral, bindValue)), variables);
                    }

                    break;

                case ExpressionStatement exprStatement:
                    ExecuteExpression(exprStatement.Expression, variables);
                    break;


                default: throw new NotImplementedException();
            }
        }

        public static object ExecuteExpression(IExpression expression, Dictionary<string, IExpression> variables)
        {
            switch (expression)
            {
                case Literal literal: return literal.Value;
                case Identifier identifier: return ExecuteExpression(variables[identifier.Id], variables);

                case Unary unary:

                    return unary.Expression.Type switch
                    {
                        DataType.Numeral => unary.Operator.Type switch
                        {
                            TokenType.Minus => -(double)ExecuteExpression(unary.Expression, variables),
                            TokenType.Plus => ExecuteExpression(unary.Expression, variables),
                            _ => throw new InterpretException($"Oporator {unary.Operator.Lexeme} not supported for type Numeric")
                        },
                        DataType.Bool => unary.Operator.Type switch
                        {
                            TokenType.Not => !(bool)ExecuteExpression(unary.Expression, variables),
                            _ => throw new InterpretException($"Oporator {unary.Operator.Lexeme} not supported for type Bool")
                        },
                        _ => throw new InterpretException($"Oporator {unary.Operator.Lexeme} not supported for type {unary.Expression.Type}")

                    };

                case And and:
                    if ((bool)ExecuteExpression(and.Left, variables))
                        return ExecuteExpression(and.Right, variables);
                    else
                        return false;

                case Binary binary:

                    var leftResult = ExecuteExpression(binary.Left, variables);
                    var rightResult = ExecuteExpression(binary.Right, variables);

                    return binary.Operator.Type switch
                    {
                        //strings
                        _ when leftResult is string leftString && rightResult is string rightString =>
                             binary.Operator.Type switch
                             {
                                 TokenType.Plus => leftString + rightString,

                                 TokenType.Equal => leftString == rightString,
                                 TokenType.NotEqual => leftString != rightString,

                                 _ => throw new InterpretException(
                                     binary.Operator.Line,
                                     binary.Operator.Character,
                                     $"Oporator \"{binary.Operator.Lexeme}\" is not valid between strings")
                             },

                        //numerals
                        _ when leftResult is double leftDouble && rightResult is double rightDouble =>
                             binary.Operator.Type switch
                             {
                                 TokenType.Plus => leftDouble + rightDouble,
                                 TokenType.Minus => leftDouble - rightDouble,
                                 TokenType.Star => leftDouble * rightDouble,
                                 TokenType.Slash => leftDouble / rightDouble,
                                 TokenType.GreaterThen => leftDouble > rightDouble,
                                 TokenType.GreaterOrEqual => leftDouble >= rightDouble,
                                 TokenType.LessThen => leftDouble < rightDouble,
                                 TokenType.LessOrEqual => leftDouble <= rightDouble,

                                 TokenType.Equal => leftDouble == rightDouble,
                                 TokenType.NotEqual => leftDouble != rightDouble,

                                 _ => throw new InterpretException(
                                    binary.Operator.Line,
                                    binary.Operator.Character,
                                    $"Oporator {binary.Operator.Type} is not valid between numerals")
                             },

                        // bools
                        _ when leftResult is bool leftBool && rightResult is bool rightBool =>
                        binary.Operator.Type switch
                        {
                            TokenType.Equal => leftBool == rightBool,
                            TokenType.NotEqual => leftBool != rightBool,

                            TokenType.Or => leftBool || rightBool,

                            _ => throw new InterpretException(
                                    binary.Operator.Line,
                                    binary.Operator.Character,
                                    $"Oporator {binary.Operator.Type} is not valid between bools")
                        },

                        TokenType.Equal => ExecuteExpression(binary.Left, variables) == ExecuteExpression(binary.Right, variables),
                        TokenType.NotEqual => ExecuteExpression(binary.Left, variables) != ExecuteExpression(binary.Right, variables),

                        _ => throw new InterpretException(
                                    binary.Operator.Line,
                                    binary.Operator.Character,
                                    $"Oporator {binary.Operator.Type} is not valid between types {leftResult.GetType()} and {rightResult.GetType()}")
                    };

                case When whenExpr:

                    return ExecuteWhen(whenExpr);

                    object ExecuteWhen(When when)
                    {
                        var ifExpression = when.IfExpresion != null ? ExecuteExpression(when.IfExpresion, variables) : null;

                        return ifExpression switch
                        {
                            null => ExecuteExpression(when.ResultExpression, variables),
                            bool boolResult => boolResult ? ExecuteExpression(when.ResultExpression, variables) : ExecuteWhen(when.ElseWhen),
                        };
                    }

                case ExpressionBlock expressionBlock:
                    return (ExecuteProgram(expressionBlock.Statements, new(variables)) as Pass<object>).Value;

                case Function function:
                    return new FrostFunction(function, new(variables));

                case ICallable icallable:
                    return icallable;

                case Call call:

                    var callee = ExecuteExpression(call.Callee, variables);
                    if (callee is ICallable callable)
                        return call.Argument is not null ? 
                            callable.Call(ExecuteExpression(call.Argument, variables)) :
                            callable.Call(null);
                    else
                        throw new InterpretException($"Expression of type {callee.GetType()} is not callable");
                        //return callee;

                default: throw new NotImplementedException();
            };
        }
    }
}