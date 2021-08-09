using FrostScript.DataTypes;
using FrostScript.Expressions;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace FrostScript
{
    public static class Interpreter
    {
        public static readonly Func<Dictionary<string, IExpression>, Func<IExpression[], Result>> interpret = nativeFunctions => expressions =>
        {
            try
            {
                object result = null;
                foreach (var expr in expressions)
                    result = ExecuteExpression(expr, nativeFunctions);

                return Result.Pass(result);
            }
            catch (InterpretException ex)
            {
                Reporter.Report(ex.Line, ex.CharacterPos, ex.Message);
                return Result.Fail();
            }
        };


        public static dynamic ExecuteExpression(IExpression expression, Dictionary<string, IExpression> variables)
        {
            switch (expression)
            {
                case Bind bind:
                    variables[bind.Id] = new Literal(bind.Value.Type, ExecuteExpression(bind.Value, variables));
                    return null;

                case Assign assign:
                    variables[assign.Id] = new Literal(assign.Value.Type, ExecuteExpression(assign.Value, variables));
                    return null;

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
                        TokenType.Or => leftResult || rightResult,
                    };

                case When whenExpr:

                    var whenResult = whenExpr.Clauses.FirstOrDefault(x => ExecuteExpression(x.BoolExpression, variables) is null or true).ResultExpression;

                    return whenResult is not null ? ExecuteExpression(whenResult, variables) : null;
                   
                case ExpressionBlock expressionBlock:
                    foreach (var bodyExpression in expressionBlock.Body.SkipLast(1))
                        ExecuteExpression(bodyExpression, variables);

                    var x = ExecuteExpression(expressionBlock.Body.Last(), variables);
                    return x;

                case Function function:
                    return new FrostFunction(function, new(variables));

                case ICallable icallable:
                    return icallable;

                case Call call:

                var callee = ExecuteExpression(call.Callee, variables);

                var callable = callee as ICallable;
                return callable.Call(ExecuteExpression(call.Argument, variables));

                case Loop loop:

                    if (loop.Bind is not null)
                        ExecuteExpression(loop.Bind, variables);

                    var values = GetValue();
                    IEnumerable<dynamic> GetValue()
                    {

                        while (loop.Condition is null || (bool)ExecuteExpression(loop.Condition, variables))
                        {
                            foreach (var bodyExpression in loop.Body)
                            {
                                if (bodyExpression is Yield yield)
                                    yield return ExecuteExpression(yield.Value, variables);
                                else
                                    ExecuteExpression(bodyExpression, variables);
                            }

                            if (loop.Assign is not null)
                                ExecuteExpression(loop.Assign, variables);
                        }
                    }

                    return values.Any() ? values.ToArray() : null;


                default: throw new NotImplementedException($"{expression}");
            };
        }
    }
}