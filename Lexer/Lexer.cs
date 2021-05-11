using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class Lexer
    {
        public static IEnumerable<Token> GetTokens(string sourceCode)
        {
            var characters = sourceCode.ToCharArray();

            var line = 1;
            var characterPos = 0;

            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];
                characterPos++;
                switch (character)
                {
                    case '(': yield return new(TokenType.ParentheseOpen, line, characterPos, character.ToString()); break;
                    case ')': yield return new(TokenType.ParentheseClose, line, characterPos, character.ToString()); break;
                    case '{': yield return new(TokenType.BraceOpen, line, characterPos, character.ToString()); break;
                    case '}': yield return new(TokenType.BraceClose, line, characterPos, character.ToString()); break;
                    case ',': yield return new(TokenType.Comma, line, characterPos, character.ToString()); break;
                    case '.': yield return new(TokenType.Dot, line, characterPos, character.ToString()); break;
                    case '+': yield return new(TokenType.Plus, line, characterPos, character.ToString()); break;
                    case ';': yield return new(TokenType.NewLine, line, characterPos, character.ToString()); break;
                    case '*': yield return new(TokenType.Star, line, characterPos, character.ToString()); break;
                    case '|': yield return new(TokenType.Pipe, line, characterPos, character.ToString()); break;
                    case '-':
                        if (characters.Skip(i + 1).Contains('>'))
                        {
                            yield return new(TokenType.Arrow, line, characterPos + 1, character.ToString());
                            i++;
                            characterPos++;
                        }
                        else yield return new(TokenType.Minus, line, characterPos, character.ToString());

                        break;

                    case '!': yield return Match('=') ? new(TokenType.NotEqual, line, characterPos, character.ToString()) : new(TokenType.Not, line, characterPos, character.ToString()); break;
                    case '=': yield return Match('=') ? new(TokenType.Equal, line, characterPos, character.ToString()) : new(TokenType.Assign, line, characterPos, character.ToString()); break;
                    case '<': yield return Match('=') ? new(TokenType.LessOrEqual, line, characterPos, character.ToString()) : new(TokenType.LessThen, line, characterPos, character.ToString()); break;
                    case '>': yield return Match('=') ? new(TokenType.GreaterOrEqual, line, characterPos, character.ToString()) : new(TokenType.GreaterThen, line, characterPos, character.ToString()); break;

                    //string litteral
                    case '"':
                        if (!characters.Skip(i + 1).Contains('"'))
                            Reporter.Report(line, i + 1, $"string literal was not closed");
                        var stringCharacters = characters.Skip(i + 1).TakeWhile(x => x != '"').ToArray();
                        var stringLit = new string(stringCharacters);
                        yield return new Token(TokenType.String, line, characterPos, stringLit, stringLit);

                        i += stringCharacters.Length + 1;
                        break;

                    //numeral litteral
                    case char _ when char.IsDigit(character):
                        var digits = new string(characters.Skip(i).TakeWhile(x => char.IsDigit(x) || x == '.').ToArray());

                        yield return new Token(TokenType.Numeral, line, characterPos, digits, double.Parse(digits));
                        i += digits.Length - 1;

                        //add a multiplication if numeric is followed by a parenthese
                        if (i + 1 < characters.Length && characters[i + 1] == '(')
                            yield return new Token(TokenType.Star, line, character);

                        break;

                    //ids and reserved words
                    case char _ when char.IsLetter(character):
                        var word = new string(characters.Skip(i).TakeWhile(x => char.IsLetterOrDigit(x)).ToArray());
                        yield return word switch
                        {
                            "if" => new Token(TokenType.If, line, characterPos, word),
                            "else" => new Token(TokenType.Else, line, characterPos, word),
                            "when" => new Token(TokenType.When, line, characterPos, word),

                            "print" => new Token(TokenType.Print, line, characterPos, word),

                            "true" => new Token(TokenType.True, line, characterPos, word, true),
                            "false" => new Token(TokenType.False, line, characterPos, word, false),
                            "null" => new Token(TokenType.Null, line, characterPos, word),

                            "for" => new Token(TokenType.For, line, characterPos, word),
                            "while" => new Token(TokenType.While, line, characterPos, word),

                            "var" => new Token(TokenType.Var, line, characterPos, word),
                            "let" => new Token(TokenType.Let, line, characterPos, word),

                            //new id
                            _ => new Token(TokenType.Id, line, i + 1, word)
                        };

                        i += word.Length - 1;
                        break;


                    //comment
                    case '/':
                        if (Match('/'))
                        {
                            //skip to end of comment
                            i += characters.Skip(i).TakeWhile(x => x != '\n').Count();

                            continue;
                        }
                        else
                            yield return new(TokenType.Slash, line, characterPos, character.ToString());
                        break;

                    //ignore white space
                    case ' ':
                    case '\r':
                    case '\t':
                        break;

                    case '\n': line++; characterPos = 0; break;

                    default: Reporter.Report(line, i + 1, $"Charactor {character} not supported"); break;
                }

                bool Match(char expected)
                {
                    if (i + 1 >= characters.Length)
                        return false;

                    if (characters[i + 1] != expected)
                        return false;

                    i++;
                    return true;
                }
            }

            yield return new Token(TokenType.Eof, line, 0);
        }
    }
}