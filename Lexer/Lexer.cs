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
            //remove white space
            var characters = text
                .ToCharArray()
                .Where(x => x != ' ')
                .ToArray();

            using var reader = new StringReader(new string(characters));

            while (reader.Peek() != -1)
            {
                var character = (char)reader.Read();

                yield return character switch
                {
                    '+' or '-' or '*' or '/' => new Token(TokenType.Operator, character.ToString()),
                    ';' or '\n' => new Token(TokenType.NewLine, character.ToString()),

                    '(' or '[' or '{' => new Token(TokenType.ParentheseOpen, character.ToString()),
                    ')' or ']' or '}' => new Token(TokenType.ParentheseClose, character.ToString()),

                    '=' => new Token(TokenType.Assign, character.ToString()),

                    _ when char.IsLetter(character) => new Token(TokenType.Id, GetFullId(character, reader)),

                    char digit when char.IsDigit(character) => new Token(TokenType.Integer, GetFullInteger(character, reader)),
                    _ => throw new Exception($"Charactor {character} not supported")
                };
            }

            static string GetFullId(char firstChar, StringReader reader)
            {
                return new string(Step().ToArray());

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