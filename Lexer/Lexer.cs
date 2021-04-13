using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Interpreter
{
    public static class Lexer
    {
        public static IEnumerable<Token> Tokenize(string text)
        {
            using var reader = new StringReader(text);

            while (reader.Peek() != -1)
            {
                var character = (char)reader.Read();

                yield return character switch
                {
                    '+' or '-' or '*' or '/' => new Token(TokenType.Operator, character.ToString()),
                    ';' or '\n' or '\r' => new Token(TokenType.NewLine, character.ToString()),

                    '(' or '[' or '{' => new Token(TokenType.ParentheseOpen, character.ToString()),
                    ')' or ']' or '}' => new Token(TokenType.ParentheseClose, character.ToString()),

                    '=' => new Token(TokenType.Assign, character.ToString()),

                    ' '  => new Token(TokenType.Discard, ""),
                    _ when char.IsLetter(character) => HandleLetters(character, reader),

                    char digit when char.IsDigit(character) => new Token(TokenType.Integer, GetFullInteger(character, reader)),
                    _ => throw new Exception($"Charactor {character} not supported")
                };
            }

            Token HandleLetters(char firstChar, StringReader reader)
            {
                var word = new string(Step().ToArray());

                return word switch
                {
                    "print" => new Token(TokenType.Print, word),
                    _ => new Token(TokenType.Id, word)
                };

                IEnumerable<char> Step()
                {
                    yield return firstChar;

                    while (char.IsLetterOrDigit((char)reader.Peek()))
                    {
                        yield return (char)reader.Read();
                    }
                }
            }

            static string GetFullInteger(char firstInt, StringReader reader)
            {
                return new string(Step().ToArray());

                IEnumerable<char> Step()
                {
                    yield return firstInt;

                    while (char.IsDigit((char)reader.Peek()))
                    {
                        yield return (char)reader.Read();
                    }
                }
            }
        }
    }
}