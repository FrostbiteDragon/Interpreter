using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Interpreter
{
    public static class Lexer
    {
        private static Token HandleMacAndWindowsReturn(StringReader reader, char character)
        {
            // Check the next character.
            if ((char)reader.Peek() == '\n')
            {
                // Read out the \n so it doesn't throw an exception on the next iteration.
                var disposeChar = (char)reader.Read();
                return new Token(TokenType.NewLine, character.ToString());
            }
            else
                return new Token(TokenType.NewLine, character.ToString());
        }

        private static Token CreateToken(StringReader reader, char character)
        {
            return character switch
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

                // End-of-line check for Mac and Window systems.
                //  If the next character is an \n, then this is a Windows system.
                //  Else we just have an \r, this is a Mac system and we just continue.
                // ~Zach
                if (character == '\r')
                    yield return HandleMacAndWindowsReturn(reader, character);
                else
                    yield return CreateToken(reader, character);
            }
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