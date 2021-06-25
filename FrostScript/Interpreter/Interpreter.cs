using FrostScript.DataTypes;
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
        public static readonly Stopwatch Stopwatch = new Stopwatch();

        public static Result ExecuteProgram(IEnumerable<IStatement> statements, Dictionary<string, IExpression> variables = null)
        {
            Stopwatch.Start();

            if (variables is null)
                variables = new Dictionary<string, IExpression>();

            try
            {
                if (!statements.Any())
                    return Result.Pass();

                if (statements.Last() is ExpressionStatement result)
                {
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
                    variables[id] = new Literal(value.Type, ExecuteExpression(value, variables));
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

                        ExecuteStatement(new Assign(@for.Bind.Id, new Literal(DataType.Int, bindValue)), variables);
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

                    dynamic result = ExecuteExpression(unary.Expression, variables);

                    return unary.Operator.Type switch
                    {
                        TokenType.Minus => -result,
                        TokenType.Plus => +result,
                        TokenType.Not => !result
                    };


                case And and:
                    if ((bool)ExecuteExpression(and.Left, variables))
                        return ExecuteExpression(and.Right, variables);
                    else
                        return false;

                case Binary binary:

                    dynamic leftResult = ExecuteExpression(binary.Left, variables);
                    dynamic rightResult = ExecuteExpression(binary.Right, variables);

                    return binary.Operator.Type switch
                    {
                        TokenType.Plus => leftResult + rightResult,
                        TokenType.Minus => leftResult - rightResult,
                        TokenType.Star => leftResult * rightResult,
                        TokenType.Slash => leftResult / rightResult,
                        TokenType.GreaterThen => leftResult > rightResult,
                        TokenType.GreaterOrEqual => leftResult >= rightResult,
                        TokenType.LessThen => leftResult < rightResult,
                        TokenType.LessOrEqual => leftResult <= rightResult,
                        TokenType.Equal => leftResult == rightResult,
                        TokenType.NotEqual => leftResult != rightResult,
                        TokenType.Or => leftResult || rightResult
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

                case Expressions.Function function:
                    return new FrostFunction(function, new(variables));

                case ICallable icallable:
                    return icallable;

                case Call call:

                    var callee = ExecuteExpression(call.Callee, variables);

                    if (callee is ICallable callable)
                        return callable.Call(ExecuteExpression(call.Argument, variables));
                    else
                        throw new InterpretException($"Expression of type {callee?.GetType().ToString() ?? "void"} is not callable");
                        //return callee;

                default: throw new NotImplementedException();
            };
        }
    }
}