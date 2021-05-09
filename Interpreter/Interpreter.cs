using FrostScript.Expressions;
using FrostScript.Statements;
using System;
using System.Collections.Generic;

namespace FrostScript
{
    public static class Interpreter
    {
        static readonly Dictionary<string, object> variables = new Dictionary<string, object>();

        public static void ExecuteProgram(IEnumerable<Statement> statements)
        {
            try
            {
                foreach (var statement in statements)
                    ExecuteStatement(statement);
            }
            catch (InterpretException exception)
            {
                Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
            }
        }

        private static void ExecuteStatement(Statement statement)
        {
            switch (statement)
            {
                case Print print:
                    Console.WriteLine(ExecuteExpression(print.Expression));
                    break;
                case Bind (var id, var value):
                    variables[id] = ExecuteExpression(value);
                    break;


                case ExpressionStatement exprStatement: break;
            }
        }

        private static object ExecuteExpression(Expression expression)
        {
            switch (expression)
            {
                case Literal literal: return literal.Value;
                case Identifier identifier: return variables[identifier.Id]; 

                case Binary binary:

                    var leftResult = ExecuteExpression(binary.Left);
                    var rightResult = ExecuteExpression(binary.Right);

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

                            _ => throw new InterpretException(
                                    binary.Operator.Line,
                                    binary.Operator.Character,
                                    $"Oporator {binary.Operator.Type} is not valid between bools")
                        },

                        TokenType.Equal => ExecuteExpression(binary.Left) == ExecuteExpression(binary.Right),
                        TokenType.NotEqual => ExecuteExpression(binary.Left) != ExecuteExpression(binary.Right),

                        _ => throw new InterpretException(
                                    binary.Operator.Line,
                                    binary.Operator.Character,
                                    $"Oporator {binary.Operator.Type} is not valid between types {leftResult.GetType()} and {rightResult.GetType()}")
                    };

                case When whenExpr:

                    return ExecuteWhen(whenExpr);

                    object ExecuteWhen(When when)
                    {
                        var ifExpression = when.IfExpresion != null ? ExecuteExpression(when.IfExpresion) : null;

                        return ifExpression switch
                        {
                            null => ExecuteExpression(when.ResultExpression),
                            bool boolResult => boolResult ? ExecuteExpression(when.ResultExpression) : ExecuteWhen(when.ElseWhen),
                        };
                    }

                case null: throw new InterpretException("Errors must be addressed before interpretation can commence");  break;

                default: throw new NotImplementedException();
            };
        }
    }
}
