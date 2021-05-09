using FrostScript.Expressions;
using FrostScript.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class Parser
    {
        public static IEnumerable<Statement> GenerateAST(Token[] tokens)
        {
            int currentPosition = 0;
            while (tokens[currentPosition].Type != TokenType.Eof)
            {
                var (statement, newPos) = TryGetStatement(currentPosition, tokens);

                currentPosition = newPos;

                if (statement is not null)
                    yield return statement;
            }

            (Statement statement, int newPos) TryGetStatement(int pos, Token[] tokens)
            {
                try 
                {
                    var (statement, newPos) = GetStatement(pos, tokens);

                    return (statement, newPos); 
                }
                catch (ParseException exception)
                {
                    Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
                    return (null, pos + 1);
                }
            }

            (Statement statement, int newPos) GetStatement(int pos, Token[] tokens)
            {
                return tokens[pos].Type switch
                {
                    TokenType.Print => GetPrint(pos + 1, tokens),
                    TokenType.NewLine => GetStatement(pos + 1, tokens),
                    TokenType.Var => GetBind(pos + 1, tokens),
                    _ => GetExpressionStatement(pos, tokens)
                };


            }

            (Statement statement, int newPos) GetExpressionStatement(int pos, Token[] tokens)
            {
                var (expression, newPos) = GetExpression(pos, tokens);

                return (new ExpressionStatement(expression), newPos);
            }

            (Statement statement, int newPos) GetPrint(int pos, Token[] tokens)
            {
                var (expression, newPos) = GetExpression(pos, tokens);

                return (new Print(expression), newPos);
            }

            (Statement statement, int newPos) GetBind(int pos, Token[] tokens)
            {
                var id = tokens[pos].Type switch
                {
                    TokenType.Id => tokens[pos].Lexeme,
                    _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected Id")
                };

                //check for '='
                if (tokens[pos + 1].Type is not TokenType.Assign)
                    throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected '='");

                var (value, newPos) = GetExpression(pos + 2, tokens);
                return (new Bind(id, value), newPos);

            }


            (Expression expression, int newPos) GetExpression(int pos, Token[] tokens)
            {
                return When(pos, tokens);
            }

            (Expression expression, int newPos) When(int pos, Token[] tokens)
            {
                if (tokens[pos].Type is not TokenType.When)
                    return Equality(pos, tokens);

                pos++;

                return GenerateWhen(null, pos, tokens);

                (When when, int pos) GenerateWhen(When when, int pos, Token[] tokens)
                {
                    if (pos >= tokens.Length)
                        throw new ParseException(tokens.Last().Line, tokens.Last().Character, $"Missing default clause");

                    if (tokens[pos].Type is TokenType.Pipe)
                    {
                        if (tokens[pos + 1].Type is not TokenType.Arrow)
                        {
                            var boolResult = GetExpression(pos + 1, tokens);

                            if (tokens[boolResult.newPos].Type is not TokenType.Arrow)
                                throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"expected \"->\" ");

                            var (expression, newPos) = GetExpression(boolResult.newPos + 1, tokens);
                            var newWhen = new When(boolResult.expression, expression, null);

                            if (when is null)
                                return GenerateWhen(newWhen, newPos, tokens);
                            else
                            {
                                var result = GenerateWhen(newWhen, newPos, tokens);
                                return (new When()
                                {
                                    IfExpresion = when.IfExpresion,
                                    ResultExpression = when.ResultExpression,
                                    ElseWhen = result.when
                                }, result.pos);
                            }
                        }
                        else if (tokens[pos + 1].Type is TokenType.Arrow)
                        {
                            var (expression, newPos) = GetExpression(pos + 2, tokens);
                            var newWhen = when switch
                            {
                                null => new When
                                {
                                    ResultExpression = expression
                                },
                                _ => new When
                                {
                                    IfExpresion = when.IfExpresion,
                                    ResultExpression = when.ResultExpression,
                                    ElseWhen = new When { ResultExpression = expression }
                                }
                            };
                            return (newWhen, newPos);
                        }
                        else throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Missing default clause");
                    }
                    else throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"expected \"|\"");
                }
            }

            (Expression expression, int newPos) Equality(int pos, Token[] tokens)
            {
                var (expression, newPos) = Comparison(pos, tokens);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Equal or TokenType.NotEqual)
                {
                    var result = Comparison(newPos + 1, tokens);

                    expression = new Binary(expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Comparison(int pos, Token[] tokens)
            {
                var (expression, newPos) = Term(pos, tokens);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.GreaterThen or TokenType.GreaterOrEqual or TokenType.LessThen or TokenType.LessOrEqual)
                {
                    var result = Term(newPos + 1, tokens);

                    expression = new Binary(expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }


            (Expression expression, int newPos) Term(int pos, Token[] tokens)
            {
                var (expression, newPos) = Factor(pos, tokens);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Plus or TokenType.Minus)
                {
                    var result = Factor(newPos + 1, tokens);

                    expression = new Binary(expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Factor(int pos, Token[] tokens)
            {
                var (expression, newPos) = Unary(pos, tokens);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Star or TokenType.Slash)
                {
                    var result = Unary(newPos + 1, tokens);

                    expression = new Binary(expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Unary(int pos, Token[] tokens)
            {
                if (tokens[pos].Type is TokenType.Minus or TokenType.Not)
                {
                    var (expression, newPos) = Primary(pos + 1, tokens);
                    return (new Unary(tokens[pos], expression), newPos);
                }
                else
                {
                    return Primary(pos, tokens);
                }
            }

            (Expression expression, int newPos) Primary (int pos, Token[] tokens)
            {
                return tokens[pos].Type switch
                {
                    TokenType.True or 
                    TokenType.False or
                    TokenType.Null or
                    TokenType.Numeral or
                    TokenType.String => (new Literal(tokens[pos].Literal), pos + 1),
                    TokenType.Id => (new Identifier(tokens[pos].Lexeme), pos + 1),

                    TokenType.ParentheseOpen => Grouping(pos, tokens),
                    _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected an expression")
                };
            }

            (Expression expression, int newPos) Grouping(int pos, Token[] tokens)
            {
                var (expression, newPos) = GetExpression(pos + 1, tokens);
                if (newPos >= tokens.Length || tokens[newPos].Type != TokenType.ParentheseClose)
                {
                    Reporter.Report(tokens[pos].Line, tokens[pos].Character, $"Parenthese not closed");
                    return (expression, newPos);
                }
                else return (expression, newPos + 1);
            }
        }
    }
}
