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

        public static readonly Func<Dictionary<string, IExpression>, Func<IExpression[], Result>> interpret = nativeFunctions => expressions =>
        {
            try
            {
                foreach (var expr in expressions)
                    ExecuteExpression(expr, nativeFunctions);
                return Result.Pass();
            }
            catch (InterpretException ex)
            {
                Reporter.Report(ex.Line, ex.CharacterPos, ex.Message);
                return Result.Fail();
            }
        };

        public static object ExecuteExpression(IExpression expression, Dictionary<string, IExpression> variables)
        {
            switch (expression)
            {
                case Bind bind:
                    variables[bind.Id] = bind.Value;
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
                    return (interpret(new(variables))(expressionBlock.Body) as Pass<object>).Value;

                case Function function:
                    return new FrostFunction(function, new(variables));

                case ICallable icallable:
                    return icallable;

                case Call call:

                var callee = ExecuteExpression(call.Callee, variables);

                var callable = callee as ICallable;
                return callable.Call(ExecuteExpression(call.Argument, variables));

            //        case While @while:
            //            while ((bool)ExecuteExpression(@while.Condition, variables))
            //            {
            //                foreach (var bodyStatement in @while.Body)
            //                    ExecuteStatement(bodyStatement, variables);
            //            }

            //            break;

            //        case For @for:

            //            //ExecuteStatement(@for.Bind, variables);

            //            //var bindValue = (double)ExecuteExpression(@for.Bind.Value, variables);

            //            //while ((bool)ExecuteExpression(@for.EndExpression, variables))
            //            //{
            //            //    foreach (var bodyStatement in @for.Body)
            //            //        ExecuteStatement(bodyStatement, variables);

            //            //    bindValue += @for.Crement switch
            //            //    {
            //            //        Crement.Increment => 1,
            //            //        Crement.Decrement => -1,
            //            //        _ => throw new ArgumentOutOfRangeException(nameof(@for.Crement))
            //            //    };

            //            //    ExecuteStatement(new Assign(@for.Bind.Id, new Literal(DataType.Int, bindValue)), variables);
            //            //}

            //            break;


                default: throw new NotImplementedException();
            };
        }
    }
}