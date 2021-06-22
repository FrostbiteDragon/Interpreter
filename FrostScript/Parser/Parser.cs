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
        public static Result GetAST(Token[] tokens, Dictionary<string, IExpression> nativeFunctions)
        {
            var globalIdentifiers = nativeFunctions.ToDictionary(x => x.Key, x => (x.Value.Type, false));
            var ast = GenerateAST(tokens, globalIdentifiers).ToArray();

            return ast.Contains(null) ?
                Result.Fail() :
                Result.Pass(ast);

            IEnumerable<IStatement> GenerateAST(Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers = null)
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

            (IStatement statement, int newPos) TryGetStatement(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                try
                {
                    (IStatement statement, int newPos) = tokens[pos].Type switch
                    {
                        TokenType.NewLine => TryGetStatement(pos + 1, tokens, identifiers),
                        TokenType.Pipe => BlockStatement(pos, tokens, identifiers),
                        TokenType.Var or TokenType.Let => Bind(pos, tokens, identifiers),
                        TokenType.If => If(pos, tokens, identifiers),
                        TokenType.While => While(pos, tokens, identifiers),
                        TokenType.For => For(pos, tokens, identifiers),
                        TokenType.Id when tokens[pos + 1].Type is TokenType.Assign => Assign(pos, tokens, identifiers),
                        _ => ExpressionStatement(pos, tokens, identifiers)
                    };

                    return (statement, newPos);
                }
                catch (ParseException exception)
                {
                    Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
                    return (null, exception.PickupPoint);
                }
            }
            (IStatement statement, int newPos) BlockStatement(int initialPos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var blockIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);

                var pos = initialPos;
                return (new StatementBlock(GetBlockStatements(initialPos).ToArray()), pos);
                IEnumerable<IStatement> GetBlockStatements(int initialPos)
                {
                    while (tokens[pos].Type is TokenType.Pipe)
                    {
                        var (statement, newPos) = TryGetStatement(pos + 1, tokens, blockIdentifiers);
                        pos = newPos;

                        yield return statement;
                    }
                }
            }
            (IStatement statement, int newPos) ExpressionStatement(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = GetExpression(pos, tokens.ToArray(), identifiers);

                return (new ExpressionStatement(expression), newPos);
            }
            (IStatement statement, int newPos) Bind(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var mutability = tokens[pos].Type is TokenType.Var;

                var id = tokens[pos + 1].Type switch
                {
                    TokenType.Id => tokens[pos + 1].Lexeme,
                    _ => throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, $"Expected Id", pos + 1)
                };

                if (identifiers.ContainsKey(id))
                    throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, $"Varriable {id} already exists in scope", pos + 1);

                var blockIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);

                //add id to the identifier dictionary but not the block's
                identifiers[id] = (DataType.Unknown, mutability);

                var parameters = new List<Parameter>();
                var currentPos = pos + 2;

                //bind is not a function
                if (tokens[currentPos].Type is TokenType.Assign)
                {
                    var (value, expressionPos) = GetExpression(currentPos + 1, tokens, blockIdentifiers);
                    //set new id's value
                    identifiers[id] = (value.Type, mutability);
                    return (new Bind(id, value), expressionPos);
                }

                //bind is a function
                while (tokens[currentPos].Type is not (TokenType.Pipe or TokenType.ReturnPipe))
                {
                    var (paramater, newPos) = Parameter(currentPos, tokens, blockIdentifiers);
                    blockIdentifiers[paramater.Id] = (paramater.Type, false);
                    parameters.Add(paramater);
                    currentPos = newPos;
                }

                var (body, bodyPos) = GetExpression(currentPos, tokens, blockIdentifiers);

                //set new id's value
                identifiers[id] = (body.Type, mutability);

                if (!parameters.Any())
                    return (new Bind(id, new Function(null, body)), bodyPos);

                Function function = null;
                foreach (var parameter in parameters.Reverse<Parameter>())
                {
                    if (function is null)
                        function = new Function(parameter, body);
                    else
                    {
                        function = new Function(parameter, function);
                    }
                }

                return (new Bind(id, function), bodyPos);
            }
            (Parameter statement, int newPos) Parameter(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var id = tokens[pos].Type switch
                {
                    TokenType.Id => tokens[pos].Lexeme,
                    _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected Id", pos + 3)
                };

                if (tokens[pos + 1].Type is not TokenType.Colon)
                    throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected ':' but got {tokens[pos].Lexeme}", pos + 3);

                var type = tokens[pos + 2].Type switch
                {
                    TokenType.IntType => DataType.Numeral,
                    TokenType.StringType => DataType.String,
                    TokenType.BoolType => DataType.Bool,
                    _ => throw new ParseException(tokens[pos + 2].Line, tokens[pos + 2].Character, $"Expected type but got {tokens[pos + 2].Lexeme}", pos + 3)
                };

                return (new Parameter(id, type), pos + 3);
            }
            (IStatement statement, int newPos) Assign(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
            (IStatement statement, int newPos) If(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[pos + 1].Type is not TokenType.BraceOpen)
                    throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, "expected '{'", pos + 1);

                var (ifStmt, newPos) = GenerateIf(null, pos + 2, tokens, identifiers);

                if (tokens[newPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens[newPos].Line, tokens[newPos].Character, "expected '}'. \"when\" was never closed", newPos);

                return (ifStmt, newPos + 1);


                (If ifStmt, int pos) GenerateIf(If ifStmt, int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
                {
                    if (pos >= tokens.Length)
                        return (ifStmt, pos);

                    if (tokens[pos].Type is TokenType.BraceClose)
                        return (ifStmt, pos);

                    //default clause
                    if (tokens[pos].Type is TokenType.Arrow)
                    {
                        var (statment, newPos) = TryGetStatement(pos + 1, tokens, identifiers);

                        var newIf = ifStmt switch
                        {
                            null => new If
                            {
                                ResultStatement = statment
                            },
                            _ => new If
                            {
                                IfExpresion = ifStmt.IfExpresion,
                                ResultStatement = ifStmt.ResultStatement,
                                ElseIf = new If { ResultStatement = statment }
                            }
                        };
                        return (newIf, newPos);

                    }
                    //if clause
                    else
                    {
                        var boolResult = GetExpression(pos, tokens, identifiers);

                        if (tokens[boolResult.newPos].Type is not TokenType.Arrow)
                            throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"expected \"->\"", pos);

                        var (statement, newPos) = TryGetStatement(boolResult.newPos + 1, tokens, identifiers);
                        var newIf = new If(boolResult.expression, statement, null);

                        //end of if
                        if (tokens[newPos].Type is TokenType.BraceClose)
                        {
                            if (ifStmt is null)
                                return (newIf, newPos);
                            else
                            {
                                return (new If
                                {
                                    IfExpresion = ifStmt.IfExpresion,
                                    ResultStatement = ifStmt.ResultStatement,
                                    ElseIf = newIf
                                }, newPos);
                            }
                        }

                        if (tokens[newPos].Type is not TokenType.Comma)
                            throw new ParseException(
                                tokens[newPos].Line,
                                tokens[newPos].Character,
                                $"expected \',\'",
                                newPos + tokens.Skip(newPos).TakeWhile(x => x.Type is not TokenType.BraceClose).Count() + 1);

                        if (ifStmt is null)
                            return GenerateIf(newIf, newPos + 1, tokens, identifiers);
                        else
                        {
                            var result = GenerateIf(newIf, newPos + 1, tokens, identifiers);

                            return (new If
                            {
                                IfExpresion = ifStmt.IfExpresion,
                                ResultStatement = ifStmt.ResultStatement,
                                ElseIf = result.ifStmt
                            }, result.pos);
                        }
                    }
                }
            }
            (IStatement statement, int newPos) For(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var ifIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);

                var (bind, bindPos) = Bind(pos + 1, tokens, ifIdentifiers);

                var crement = tokens[bindPos].Type switch
                {
                    TokenType.To => Crement.Increment,
                    TokenType.DownTo => Crement.Decrement,
                    _ => throw new ParseException(
                        tokens[bindPos].Line,
                        tokens[bindPos].Character,
                        $"Expected \"to\" or \"downto\" but got {tokens[bindPos].Lexeme}",
                        bindPos + tokens.Skip(bindPos).TakeWhile(x => x.Type is not TokenType.BraceClose).Count() + 1)
                };

                var (endExpression, expressionPos) = GetExpression(bindPos + 1, tokens, ifIdentifiers);

                if (tokens[expressionPos].Type is not TokenType.BraceOpen)
                    throw new ParseException(tokens[expressionPos].Line, tokens[expressionPos].Character, $"Expected \"{{\" but got {tokens[expressionPos].Lexeme}", expressionPos);

                var statements = new List<IStatement>();
                var bodyPos = expressionPos + 1;

                while (tokens[bodyPos].Type is not TokenType.BraceClose)
                {
                    var (statement, statementPos) = TryGetStatement(bodyPos, tokens, ifIdentifiers);

                    bodyPos = statementPos;

                    if (statement is null)
                        return (null, bodyPos);
                    else
                        statements.Add(statement);
                }

                if (tokens[bodyPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens[bodyPos].Line, tokens[bodyPos].Character, $"Expected \"}}\" but got {tokens[bodyPos].Lexeme}", bodyPos);

                return (new For(bind as Bind, endExpression, crement, statements), bodyPos + 1);
            }
            (IStatement statement, int newPos) While(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (condition, expressionPos) = GetExpression(pos + 1, tokens, identifiers);

                if (tokens[expressionPos].Type is not TokenType.BraceOpen)
                    throw new ParseException(tokens[expressionPos].Line, tokens[expressionPos].Character, $"Expected \"{{\" but got {tokens[expressionPos].Lexeme}", expressionPos);

                var statements = new List<IStatement>();
                var bodyPos = expressionPos + 1;

                while (tokens[bodyPos].Type is not TokenType.BraceClose)
                {
                    var (statement, statementPos) = TryGetStatement(bodyPos, tokens, identifiers);

                    bodyPos = statementPos;

                    if (statement is null)
                        return (null, bodyPos);
                    else
                        statements.Add(statement);
                }

                if (tokens[bodyPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens[bodyPos].Line, tokens[bodyPos].Character, $"Expected \"}}\" but got {tokens[bodyPos].Lexeme}", bodyPos);

                return (new While(condition, statements), bodyPos + 1);
            }

            (IExpression expression, int newPos) GetExpression(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                return Or(pos, tokens, identifiers);
            }
            (IExpression expression, int newPos) Or(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
            (IExpression expression, int newPos) And(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
            (IExpression expression, int newPos) Equality(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
            (IExpression expression, int newPos) Comparison(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
            (IExpression expression, int newPos) Term(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
            (IExpression expression, int newPos) Factor(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = ExpressionBlock(pos, tokens, identifiers);

                while (newPos < tokens.Length && tokens[newPos].Type is TokenType.Star or TokenType.Slash)
                {
                    var result = ExpressionBlock(newPos + 1, tokens, identifiers);

                    //assume the type of the left expression [temporary]
                    expression = new Binary(expression.Type, expression, tokens[newPos], result.expression);
                    newPos = result.newPos;
                }

                return (expression, newPos);
            }
            (IExpression expression, int newPos) ExpressionBlock(int initialPos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[initialPos].Type is not (TokenType.Pipe or TokenType.ReturnPipe))
                    return When(initialPos, tokens, identifiers);

                var blockIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);

                var pos = initialPos;
                var statements = GetBlockStatements(initialPos).ToList();
                IEnumerable<IStatement> GetBlockStatements(int initialPos)
                {
                    while (tokens[pos].Type is TokenType.Pipe)
                    {
                        var (statement, newPos) = TryGetStatement(pos + 1, tokens, blockIdentifiers);
                        pos = newPos;

                        yield return statement;
                    }
                }

                if (tokens[pos].Type is not TokenType.ReturnPipe)
                    return (new ExpressionBlock(statements.Append(new ExpressionStatement(new Literal(DataType.Void, null)))), pos);
                else
                {
                    var (expression, newPos) = GetExpression(pos + 1, tokens, blockIdentifiers);
                    statements.Add(new ExpressionStatement(expression));
                    return (new ExpressionBlock(statements), newPos);
                }

            }
            (IExpression expression, int newPos) When(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[pos].Type is not TokenType.When)
                    return Function(pos, tokens, identifiers);

                if (tokens[pos + 1].Type is not TokenType.BraceOpen)
                    throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, "expected '{' after when", pos + 1);

                var (when, newPos) = GenerateWhen(null, pos + 2, tokens, identifiers);

                if (tokens[newPos].Type is not TokenType.BraceClose)
                    throw new ParseException(tokens[newPos].Line, tokens[newPos].Character, "expected '}'. \"when\" was never closed", newPos);

                if (!VerrifyWhen(when))
                    throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"All clauses in a when expression must return the same type", newPos + 1);

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
                                IfExpresion = when.IfExpresion,
                                ResultExpression = when.ResultExpression,
                                ElseWhen = result.when
                            }, result.pos);
                        }
                    }
                }
            }
            (IExpression expression, int newPos) Function(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[pos].Type is not TokenType.Fun)
                    return Unary(pos, tokens, identifiers);

                var blockIdentifiers = new Dictionary<string, (DataType Type, bool Mutable)>(identifiers);

                var parameters = new List<Parameter>();
                var currentPos = pos + 1;
                while (tokens[currentPos].Type is not TokenType.Arrow)
                {
                    var (paramater, newPos) = Parameter(currentPos, tokens, blockIdentifiers);
                    blockIdentifiers[paramater.Id] = (paramater.Type, false);
                    parameters.Add(paramater);
                    currentPos = newPos;
                }

                if (tokens[currentPos].Type is not TokenType.Arrow)
                    throw new ParseException(tokens[currentPos].Line, tokens[currentPos].Character, $"expected \"->\" but got {tokens[currentPos].Lexeme}", currentPos + 1);

                var (body, bodyPos) = GetExpression(currentPos + 1, tokens, blockIdentifiers);

                Function function = null;

                if (parameters.Count == 0)
                {
                    function = new Function(null, body);
                }
                else
                {
                    foreach (var parameter in parameters.Reverse<Parameter>())
                    {
                        if (function is null)
                            function = new Function(parameter, body);
                        else
                        {
                            function = new Function(parameter, function);
                        }
                    }
                }
                return (function, bodyPos);
            }
            (IExpression expression, int newPos) Unary(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                if (tokens[pos].Type is TokenType.Minus or TokenType.Plus or TokenType.Not)
                {
                    var (expression, newPos) = Unary(pos + 1, tokens, identifiers);
                    return (new Unary(tokens[pos], expression), newPos);
                }
                else
                {
                    return Call(pos, tokens, identifiers);
                }
            }
            (IExpression expression, int newPos) Call(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
            {
                var (expression, newPos) = Primary(pos, tokens, identifiers);

                var currentPos = newPos;
                var callee = expression;
                while (tokens[currentPos].Type is TokenType.ParentheseOpen)
                {
                    if (tokens[currentPos + 1].Type is TokenType.ParentheseClose)
                        return (new Call(callee), currentPos + 2);

                    var (argument, argumentPos) = GetExpression(currentPos + 1, tokens, identifiers);

                    if (tokens[argumentPos].Type is not TokenType.ParentheseClose)
                        throw new ParseException(tokens[argumentPos].Line, tokens[argumentPos].Character, $"Expected ')' but got {tokens[pos].Lexeme}", argumentPos + 1);

                    callee = new Call(callee, argument);
                    currentPos = argumentPos + 1;
                }
                    
                return (callee, currentPos);
            }
            (IExpression expression, int newPos) Primary(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
                    _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected an expression. instead got \"{tokens[pos].Lexeme}\"", pos + 1)
                };
            }
            (IExpression expression, int newPos) Grouping(int pos, Token[] tokens, Dictionary<string, (DataType Type, bool Mutable)> identifiers)
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
