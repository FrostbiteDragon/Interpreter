using FrostScript.Expressions;
using FrostScript.Statements;
using System;
using System.Collections.Generic;

namespace FrostScript
{
    public static class Interpreter
    {
        static readonly Dictionary<string, string> variables = new Dictionary<string, string>();

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

                case ExpressionStatement exprStatement: break;
            }
        }

        private static object ExecuteExpression(Expression expression)
        {
            switch (expression)
            {
                case Literal literal: return literal.Value;

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

            //foreach (Node node in nodes)
            //{
            //    ExecuteNode(node);
            //}

        //    void ExecuteNode(Node node)
        //    {
        //        if (node.Token.Type == TokenType.Integer)
        //            return;

        //        if (node.Token.Type == TokenType.Id)
        //        {
        //            node.Token = new(TokenType.Integer, variables[node.Token.Lexeme]);
        //            return;
        //        }

        //        else if (node.Token.Type == TokenType.Assign)
        //            ExecuteAssign(node);

        //        else if (node.Token.Type == TokenType.Print)
        //            ExecutePrint(node);

        //        else if (node.Token.Type == TokenType.Operator)
        //            ExecuteOporator(node);
        //        else
        //            throw new NotImplementedException($"no method to execute token type given: {node.Token.Type}");
        //    }

        //    void ExecuteAssign(Node node)
        //    {
        //        var variableId = node.Left.Token.Lexeme;

        //        ExecuteNode(node.Right);
        //        variables[variableId] = node.Right.Token.Lexeme;
        //    }

        //    void ExecutePrint(Node node)
        //    {
        //        ExecuteNode(node.Right);
        //        Console.WriteLine(node.Right.Token.Lexeme);
        //    }

        //    void ExecuteOporator(Node node)
        //    {
        //        if (node.Left.Token.Type == TokenType.Integer || node.Left.Token.Type == TokenType.Id)
        //        {
        //            if (node.Right.Token.Type == TokenType.Integer || node.Right.Token.Type == TokenType.Id)
        //            {
        //                node.Token = Calaps(node);
        //                node.Right = null;
        //                node.Left = null;
        //            }
        //            else if (node.Right != null)
        //            {
        //                ExecuteNode(node.Right);
        //                ExecuteNode(node);
        //            }
        //        }
        //        else if (node.Left != null)
        //        {
        //            ExecuteNode(node.Left);
        //            ExecuteNode(node);
        //        }
        //    }

        //    Token Calaps(Node node)
        //    {
        //        return node.Token.Lexeme switch
        //        {
        //            "+" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) + ParseIntOrId(node.Right.Token)).ToString()),
        //            "-" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) - ParseIntOrId(node.Right.Token)).ToString()),

        //            "*" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) * ParseIntOrId(node.Right.Token)).ToString()),
        //            "/" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) / ParseIntOrId(node.Right.Token)).ToString()),
        //            _ => throw new Exception($"Did not recognise character: {node.Token.Lexeme}")
        //        };

        //        int ParseIntOrId(Token token)
        //        {
        //            return token.Type switch
        //            {
        //                TokenType.Integer => int.Parse(token.Lexeme),
        //                TokenType.Id => int.Parse(variables[token.Lexeme]),
        //                _ => throw new Exception("Unexpected token")
        //            };
        //        }
        //    }
    }
}
