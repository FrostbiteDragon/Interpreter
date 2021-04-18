using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrostScript
{
    public static class Lexer
    {
        static void Report(int line, int charactorPos, string message)
        {
            Console.WriteLine($"[Line: {line}, Character: {charactorPos}] Error: {message}");
        }

        public static IEnumerable<Token> GetTokens(string sourceCode)
        {
            var characters = sourceCode.ToCharArray();

            var line = 1;

            for (int i = 0; i < characters.Length; i++)
            {
                var character = characters[i];

                switch (character)
                {
                    case '(': yield return new(TokenType.ParentheseOpen); break;
                    case ')': yield return new(TokenType.ParentheseClose); break;
                    case '{': yield return new(TokenType.BraceOpen); break;
                    case '}': yield return new(TokenType.BraceClose); break;
                    case ',': yield return new(TokenType.Comma); break;
                    case '.': yield return new(TokenType.Dot); break;
                    case '-': yield return new(TokenType.Minus); break;
                    case '+': yield return new(TokenType.Plus); break;
                    case ';': yield return new(TokenType.NewLine); break;
                    case '*': yield return new(TokenType.Star); break;
                    case '!': yield return Match('=') ? new(TokenType.NotEqual) : new(TokenType.Not); break;
                    case '=': yield return Match('=') ? new(TokenType.Equal) : new(TokenType.Assign); break;
                    case '<': yield return Match('=') ? new(TokenType.LessOrEqual) : new(TokenType.LessThen); break;
                    case '>': yield return Match('=') ? new(TokenType.GreaterOrEqual) : new(TokenType.GreaterThen); break;

                    //string litteral
                    case '"':
                        if (!characters.Skip(i + 1).Contains('"'))
                            Report(line, i + 1, $"string literal was not closed");
                        var stringCharacters = characters.Skip(i + 1).TakeWhile(x => x != '"').ToArray();
                        var stringLit = new string(stringCharacters);
                        yield return new Token(TokenType.String, stringLit, stringLit);

                        i += stringCharacters.Length + 1;
                        break;

                    //numeral litteral
                    case char _ when char.IsDigit(character):
                        var digits = new string(characters.Skip(i).TakeWhile(x => char.IsDigit(x) || x == '.').ToArray());

                        yield return new Token(TokenType.Numeral, digits, double.Parse(digits));
                        i += digits.Length - 1;
                        break;

                    //ids and reserved words
                    case char _ when char.IsLetter(character):
                        var word = new string(characters.Skip(i).TakeWhile(x => char.IsLetterOrDigit(x)).ToArray());
                        yield return word switch
                        {
                            "if" => new Token(TokenType.If),
                            "else" => new Token(TokenType.Else),

                            "print" => new Token(TokenType.Print),

                            "true" => new Token(TokenType.True, word, true),
                            "false" => new Token(TokenType.False, word, false),
                            "null" => new Token(TokenType.Null),

                            "for" => new Token(TokenType.For),
                            "while" => new Token(TokenType.While), 

                            //new id
                            _ => new Token(TokenType.Id, word)
                        };

                        i += word.Length;
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
                            yield return new(TokenType.Slash);
                        break;

                    //ignore white space
                    case ' ':
                    case '\r':
                    case '\t':
                        break;

                    case '\n': line++; break;

                    default: Report(line, i + 1, $"Charactor {character} not supported"); break;
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
        }

        //public static IEnumerable<Token> Tokenize(string text)
        //{
        //    using var reader = new StringReader(text);

        //    while (reader.Peek() != -1)
        //    {
        //        var character = (char)reader.Read();

        //        yield return character switch
        //        {
        //            '+' or '*' or '/' => new Token(TokenType.Operator, character.ToString()),
        //            ';' or '\n' or '\r' => new Token(TokenType.NewLine, character.ToString()),

        //            '-' => (char)reader.Peek() switch 
        //            { 
        //                '>' => new Token(TokenType.Arrow, $"{character}{(char)reader.Read()}"),
        //                _ => new Token(TokenType.Operator, character.ToString())
        //            },

        //            '(' or '[' or '{' => new Token(TokenType.ParentheseOpen, character.ToString()),
        //            ')' or ']' or '}' => new Token(TokenType.ParentheseClose, character.ToString()),

        //            '=' => new Token(TokenType.Assign, character.ToString()),

        //            ' '  => new Token(TokenType.Discard, ""),
        //            // \t == tab

        //            char letter when char.IsLetter(character) => HandleLetters(letter, reader),
        //            char digit when char.IsDigit(character) => HandleDigit(digit, reader),

        //            _ => throw new Exception($"Charactor {character} not supported")
        //        };
        //    }

        //    //Token HandleTabOrSpace(char character, StringReader reader)
        //    //{

        //    //}

        //    Token HandleLetters(char firstChar, StringReader reader)
        //    {
        //        var word = new string(Step().ToArray());

        //        return word switch
        //        {
        //            //reserverd keywords
        //            "->" => new Token(TokenType.Arrow, word),
        //            "if" => new Token(TokenType.If, word),
        //            "else" => new Token(TokenType.Else, word),

        //            "print" => new Token(TokenType.Print, word),

        //            "true" => new Token(TokenType.Bool, word),
        //            "false" => new Token(TokenType.Bool, word),


        //            //new id
        //            _ => new Token(TokenType.Id, word)
        //        };

        //        IEnumerable<char> Step()
        //        {
        //            yield return firstChar;

        //            while (char.IsLetterOrDigit((char)reader.Peek()))
        //            {
        //                yield return (char)reader.Read();
        //            }
        //        }
        //    }

        //    Token HandleDigit(char firstDigit, StringReader reader)
        //    {
        //        var digits = Step().ToArray();
        //        if (digits.Contains('.'))
        //            return new Token(TokenType.Decimal, new string(digits));
        //        else
        //            return new Token(TokenType.Integer, new string(digits));

        //        IEnumerable<char> Step()
        //        {
        //            yield return firstDigit;

        //            while (char.IsDigit((char)reader.Peek()) || (char)reader.Peek() == '.')
        //            {
        //                yield return (char)reader.Read();
        //            }
        //        }
        //    }
        //}
    }
}