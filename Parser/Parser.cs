using Frostware.Result;
using FrostScript.Expressions;
using FrostScript.Statements;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class Parser
    {
        public static Result GetAST(Token[] tokens)
        {
            var ast = GenerateAST(tokens).ToArray();

            if (ast.Contains(null))
                return Result.Fail();
            else
                return Result.Pass(ast);


            IEnumerable<Statement> GenerateAST(Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers = null)
            {
                if (identifiers is null)
                    identifiers = new Dictionary<string, (DataType Type, bool Mutable)>();

                int currentPosition = 0;
                while (currentPosition < tokens.Length && !(tokens[currentPosition].Type is TokenType.ClosePipe or TokenType.ReturnPipe or TokenType.Eof))
                {
                    var (statement, newPos) = TryGetStatement(currentPosition, tokens, identifiers);

                    currentPosition = newPos;

                    yield return statement;
                }

            }
            (Statement statement, int newPos) TryGetStatement(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                try
                {
                    (Statement statement, int newPos) = tokens[pos].Type switch
                    {
                        TokenType.NewLine => TryGetStatement(pos + 1, tokens, identifiers),
                        TokenType.Pipe => TryGetStatement(pos + 1, tokens, identifiers),
                        TokenType.Print => GetPrint(pos, tokens, identifiers),
                        TokenType.Var or TokenType.Let => GetBind(pos, tokens, identifiers),
                        TokenType.Id when tokens[pos + 1].Type is TokenType.Assign => GetAssign(pos, tokens, identifiers),
                        _ => GetExpressionStatement(pos, tokens, identifiers)
                    };

                    return (statement, newPos);
                }
                catch (ParseException exception)
                {
                    Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
                    return (null, exception.PickupPoint);
                }
            }

            (Statement statement, int newPos) GetExpressionStatement(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = GetExpression(pos, tokens.ToArray(), identifiers);

                return (new ExpressionStatement(expression), newPos);
            }

            (Statement statement, int newPos) GetPrint(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = GetExpression(pos + 1, tokens, identifiers);

                return (new Print(expression), newPos);
            }

            (Statement statement, int newPos) GetBind(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var id = tokens[pos + 1].Type switch
                {
                    TokenType.Id => tokens[pos + 1].Lexeme,
                    _ => throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, $"Expected Id", pos + 1)
                };


                if (identifiers.ContainsKey(id))
                    throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, $"Cannot bind to {id}. binding {id} already exists", pos + 1);

                //check for '='
                if (tokens[pos + 2].Type is not TokenType.Assign)
                    throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected '='", pos + 2);

                //store id regardless if exression throws error. this is to not log errors where there may not be one
                identifiers[id] = tokens[pos].Type switch
                {
                    TokenType.Var => (DataType.Unknown, true),
                    _ => (DataType.Unknown, false)
                };

                //A bind block cannot refference the id being bound to. so we remove it
                var exprIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);
                exprIdentifiers.Remove(id);

                var (value, newPos) = GetExpression(pos + 3, tokens, exprIdentifiers);

                if (value is null)
                    return (null, newPos);
                else
                {
                    //temporaly store id
                    identifiers[id] = tokens[pos].Type switch
                    {
                        TokenType.Var => (value.Type, true),
                        _ => (value.Type, false)
                    };
                }

                return (new Bind(id, value), newPos);
            }

            (Statement statement, int newPos) GetAssign(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var id = tokens[pos].Lexeme;

                if (tokens[pos + 1].Type is not TokenType.Assign)
                    throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, $"Expected '='", pos);

                if (!identifiers.ContainsKey(id))
                    throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Variable does not exist in current scope. did you forget a 'let' or 'var' binding?", pos);

                if (identifiers[id].Mutable == false)
                    throw new ParseException(tokens[pos + 2].Line, tokens[pos + 2].Character, $"let bindings are not mutable", pos + 2);

                var (value, newPos) = GetExpression(pos + 2, tokens, identifiers);

                if (identifiers[id].Type != value.Type)
                    throw new ParseException(tokens[pos + 2].Line, tokens[pos + 2].Character, $"binding {id} is of type {identifiers[id].Type}. It cannot be assigned a value of {value.Type}", pos + 2);

                identifiers[id] = (value.Type, true);

                return new(new Assign(id, value), newPos);
            }


            (Expression expression, int newPos) GetExpression(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                return Or(pos, tokens, identifiers);
            }

            (Expression expression, int newPos) Or(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = And(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Or)
                {
                    var result = And(newPos + 1, tokens, identifiers);

                    expression = new Binary(DataType.Bool, expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) And(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = Equality(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.And)
                {
                    var result = Equality(newPos + 1, tokens, identifiers);

                    expression = new And(expression, result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }


            (Expression expression, int newPos) Equality(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = Comparison(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Equal or TokenType.NotEqual)
                {
                    var result = Comparison(newPos + 1, tokens, identifiers);

                    expression = new Binary(DataType.Bool, expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Comparison(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = Term(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.GreaterThen or TokenType.GreaterOrEqual or TokenType.LessThen or TokenType.LessOrEqual)
                {
                    var result = Term(newPos + 1, tokens, identifiers);

                    expression = new Binary(DataType.Bool, expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Term(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = Factor(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Plus or TokenType.Minus)
                {
                    var result = Factor(newPos + 1, tokens, identifiers);

                    //assume the type of the left expression [temporary]
                    expression = new Binary(expression.Type, expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Factor(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = Unary(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Star or TokenType.Slash)
                {
                    var result = Unary(newPos + 1, tokens, identifiers);

                    //assume the type of the left expression [temporary]
                    expression = new Binary(expression.Type, expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }

            (Expression expression, int newPos) Unary(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[pos].Type is TokenType.Minus or TokenType.Plus or TokenType.Not)
                {
                    var (expression, newPos) = Unary(pos + 1, tokens, identifiers);
                    return (new Unary(expression.Type, tokens[pos], expression), newPos);
                }
                else
                {
                    return ExpressionBlock(pos, tokens, identifiers);
                }
            }

            (Expression expression, int newPos) ExpressionBlock(int initialPos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[initialPos].Type is not TokenType.Pipe)
                    return When(initialPos, tokens, identifiers);

                var pos = initialPos;

                var blockTokens = GetBlockTokens(tokens).ToArray();

                IEnumerable<Token> GetBlockTokens(Token[] tokens)
                {
                    var blockCount = 1;

                    for (int i = initialPos; i < tokens.Length; i++)
                    {
                        //if another block is opened
                        if (tokens[i].Type is TokenType.Pipe)
                            if (i - 1 > initialPos && tokens[i - 1].Type is TokenType.Assign or TokenType.BraceOpen or TokenType.Arrow)
                                blockCount += 1;

                        if (tokens[i].Type is TokenType.ReturnPipe)
                        {
                            blockCount -= 1;

                            yield return tokens[i];

                            if (blockCount == 0)
                                yield break;
                        }
                        else yield return tokens[i];
                    }
                }

                var blockIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);

                var statements = GenerateAST(blockTokens, blockIdentifiers).ToList();
                pos += blockTokens.Length;

                if (statements.Any(x => x is null))
                    return (null, pos);

                var (exprStatement, newPos) = GetExpressionStatement(pos, tokens, blockIdentifiers);

                pos = newPos;
                statements.Add(exprStatement);

                return new(new ExpressionBlock(statements), pos);
            }

            (Expression expression, int newPos) When(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[pos].Type is not TokenType.When)
                    return Primary(pos, tokens, identifiers);

                if (tokens[pos + 1].Type is not TokenType.BraceOpen)
                    throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, "expected '{' after when", pos + 1);

                var (when, newPos) = GenerateWhen(null, pos + 2, tokens, identifiers);

                if (tokens[newPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens[newPos].Line, tokens[newPos].Character, "expected '}'. \"when\" was never closed", newPos);

                if (!VerrifyWhen(when))
                    throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"All clauses in a when expression must return the same type", newPos);

                return (when, newPos + 1);

                bool VerrifyWhen(When when)
                {
                    if (when.ElseWhen is not null)
                    {
                        if (when.Type == when.ElseWhen.Type)
                            return VerrifyWhen(when.ElseWhen);
                        else return false;
                    }
                    else return true;
                }

                (When when, int pos) GenerateWhen(When when, int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
                {
                    if (pos >= tokens.Length)
                        throw new ParseException(tokens.Last().Line, tokens.Last().Character, $"Missing default clause", pos);

                    //default clause
                    if (tokens[pos].Type is TokenType.Arrow)
                    {
                        var (expression, newPos) = GetExpression(pos + 1, tokens, identifiers);

                        var newWhen = when switch
                        {
                            null => new When
                            {
                                Type = expression.Type,
                                ResultExpression = expression
                            },
                            _ => new When
                            {
                                Type = when.ResultExpression.Type,
                                IfExpresion = when.IfExpresion,
                                ResultExpression = when.ResultExpression,
                                ElseWhen = new When { ResultExpression = expression, Type = expression.Type }
                            }
                        };
                        return (newWhen, newPos);

                    }
                    //if clause
                    else
                    {
                        var boolResult = GetExpression(pos, tokens, identifiers);

                        if (tokens[boolResult.newPos].Type is not TokenType.Arrow)
                            throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"expected \"->\"", pos);

                        var (expression, newPos) = GetExpression(boolResult.newPos + 1, tokens, identifiers);
                        var newWhen = new When(boolResult.expression, expression, null);

                        if (tokens[newPos].Type is not TokenType.Comma)
                            throw new ParseException(
                                tokens[newPos].Line,
                                tokens[newPos].Character,
                                $"expected \',\'",
                                newPos + tokens.Skip(newPos).TakeWhile(x => x.Type is not TokenType.BraceClose).Count() + 1);

                        if (when is null)
                            return GenerateWhen(newWhen, newPos + 1, tokens, identifiers);
                        else
                        {
                            var result = GenerateWhen(newWhen, newPos + 1, tokens, identifiers);

                            return (new When
                            {
                                Type = when.ResultExpression.Type,
                                IfExpresion = when.IfExpresion,
                                ResultExpression = when.ResultExpression,
                                ElseWhen = result.when
                            }, result.pos);
                        }
                    }
                }
            }

            (Expression expression, int newPos) Primary(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                return tokens[pos].Type switch
                {
                    TokenType.True or TokenType.False => (new Literal(DataType.Bool, tokens[pos].Literal), pos + 1),
                    TokenType.Numeral => (new Literal(DataType.Numeral, tokens[pos].Literal), pos + 1),
                    TokenType.Null => (new Literal(DataType.Null, tokens[pos].Literal), pos + 1),
                    TokenType.String => (new Literal(DataType.String, tokens[pos].Literal), pos + 1),
                    TokenType.Id => identifiers.ContainsKey(tokens[pos].Lexeme) switch
                    {
                        true => (new Identifier(identifiers[tokens[pos].Lexeme].Type, tokens[pos].Lexeme), pos + 1),
                        false => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"The variable {tokens[pos].Lexeme} either does not exist or is out of scope", pos + 1)
                    },

                    TokenType.ParentheseOpen => Grouping(pos, tokens, identifiers),
                    _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected an expression", pos + 1)
                };
            }

            (Expression expression, int newPos) Grouping(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = GetExpression(pos + 1, tokens, identifiers);
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
