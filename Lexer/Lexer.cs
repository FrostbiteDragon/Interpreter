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

            var reader = new StringReader(new string(characters));

            while (reader.Peek() != -1)
            {
                var character = (char)reader.Read();

                yield return character switch
                {
                    '+' or '-' => new Token(TokenType.Operator, character.ToString()),
                    ';' or '\n' => new Token(TokenType.NewLine, character.ToString()),

                    char digit when char.IsDigit(character) => new Token(TokenType.Integer, GetFullInteger(character, reader)),
                    _ => throw new Exception("Charactor not supported")
                };
            }

            string GetFullInteger(char firstInt, StringReader reader)
            {
                IEnumerable<char> Step()
                {
                    yield return firstInt;

                    while (char.IsDigit((char)reader.Peek()))
                    {
                        yield return (char)reader.Read();
                    }
                }

                return new string(Step().ToArray());
            }
        }
    }
}