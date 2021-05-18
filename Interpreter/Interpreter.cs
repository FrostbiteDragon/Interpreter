using FrostScript.Expressions;
using FrostScript.Statements;
using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class Interpreter
    {
        public static Result ExecuteProgram(IEnumerable<IStatement> statements, Dictionary<string, IExpression> variables = null)
        {
            if (variables is null)
                variables = new Dictionary<string, IExpression>();

            try
            {
                if (statements.Last() is ExpressionStatement)
                {
                    var result = statements.Last() as ExpressionStatement;

                    foreach (var statement in statements.SkipLast(1))
                        ExecuteStatement(statement);

                    return Result.Pass(ExecuteExpression(result.Expression, variables));
                }
                else
                {
                    foreach (var statement in statements)
                        ExecuteStatement(statement);

                    return Result.Pass();
                }
            }
            catch (InterpretException exception)
            {
                Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
                return Result.Fail();
            }


            void ExecuteStatement(IStatement statement)
            {
                switch (statement)
                {
                    case Print print:
                        Console.WriteLine(ExecuteExpression(print.Expression, variables));
                        break;

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
                                ExecuteStatement(ifStmt.ResultStatement);
                            //if clause is true execute
                            else if ((bool)ifExpression)
                                ExecuteStatement(ifStmt.ResultStatement);
                            //else check next clause
                            else
                                ExecuteIf(ifStmt.ElseIf);
                        }
                        break;

                    case StatementBlock statementBlock:
                        foreach (var blockStatement in statementBlock.Statements)
                            ExecuteStatement(blockStatement);

                        break;

                    case While @while:
                        while ((bool)ExecuteExpression(@while.Condition, variables))
                        {
                            foreach (var bodyStatement in @while.Body)
                                ExecuteStatement(bodyStatement);
                        }

                        break;

                    case For @for:

                        ExecuteStatement(@for.Bind);

                        var bindValue = (double)ExecuteExpression(@for.Bind.Value, variables);

                        while ((bool)ExecuteExpression(@for.EndExpression, variables))
                        {
                            foreach (var bodyStatement in @for.Body)
                                ExecuteStatement(bodyStatement);

                            bindValue += @for.Crement switch
                            {
                                Crement.Increment => 1,
                                Crement.Decrement => -1,
                                _ => throw new ArgumentOutOfRangeException(nameof(@for.Crement))
                            };

                            ExecuteStatement(new Assign(@for.Bind.Id, new Literal(DataType.Numeral, bindValue)));
                        }
                     
                        break;

                    case ExpressionStatement exprStatement: break;


                    default: throw new NotImplementedException();
                }
            }

            object ExecuteExpression(IExpression expression, Dictionary<string, IExpression> variables)
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
                        return function;


                    case PartiallyAppliedFunction partialFunc:
                        foreach (var argument in partialFunc.Arguments)
                            variables[argument.Key] = argument.Value;

                        return partialFunc.Body;

                    case Call call:
                        var arguments = Array.Empty<IExpression>();

                        var callee = call as IExpression;
                        while (callee is Call childCall)
                        {
                            arguments = arguments.Prepend(childCall.Argument).ToArray();
                            callee = childCall.Callee;
                        }

                        var functionVariables = new Dictionary<string, IExpression>(variables);

                        var i = 0;
                        object result = callee switch
                        {
                            Identifier funcId when callee is Identifier  => ExecuteExpression(funcId, variables) switch 
                            {
                                PartiallyAppliedFunction partialFunc => ExecuteExpression(partialFunc, functionVariables),
                                IExpression expr => expr
                            },
                            _ => callee
                        };
                        while (i < arguments.Length && result is Function func)
                        {
                            functionVariables[func.Parameter.Id] = arguments[i];
                            
                            result = ExecuteExpression(func.Body, functionVariables);
                            i++;
                        }

                        return result is Function bodyFunc ?
                            new PartiallyAppliedFunction(functionVariables.Except(variables).ToDictionary(x => x.Key, x => x.Value), bodyFunc) 
                            : result;

                    default: throw new NotImplementedException();
                };
            }
        }
    }
}
