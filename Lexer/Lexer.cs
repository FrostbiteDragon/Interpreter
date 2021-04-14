using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace FrostScript
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
                    '+' or '*' or '/' => new Token(TokenType.Operator, character.ToString()),
                    ';' or '\n' or '\r' => new Token(TokenType.NewLine, character.ToString()),

                    '-' => (char)reader.Peek() switch 
                    { 
                        '>' => new Token(TokenType.Arrow, $"{character}{(char)reader.Read()}"),
                        _ => new Token(TokenType.Operator, character.ToString())
                    },

                    '(' or '[' or '{' => new Token(TokenType.ParentheseOpen, character.ToString()),
                    ')' or ']' or '}' => new Token(TokenType.ParentheseClose, character.ToString()),

                    '=' => new Token(TokenType.Assign, character.ToString()),

                    ' '  => new Token(TokenType.Discard, ""),
                    // \t == tab

                    char letter when char.IsLetter(character) => HandleLetters(letter, reader),
                    char digit when char.IsDigit(character) => HandleDigit(digit, reader),

                    _ => throw new Exception($"Charactor {character} not supported")
                };
            }

            //Token HandleTabOrSpace(char character, StringReader reader)
            //{

            //}

            Token HandleLetters(char firstChar, StringReader reader)
            {
                var word = new string(Step().ToArray());

                return word switch
                {
                    //reserverd keywords
                    "->" => new Token(TokenType.Arrow, word),
                    "if" => new Token(TokenType.If, word),
                    "else" => new Token(TokenType.Else, word),

                    "print" => new Token(TokenType.Print, word),

                    "true" => new Token(TokenType.Bool, word),
                    "false" => new Token(TokenType.Bool, word),


                    //new id
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

            Token HandleDigit(char firstDigit, StringReader reader)
            {
                var digits = Step().ToArray();
                if (digits.Contains('.'))
                    return new Token(TokenType.Decimal, new string(digits));
                else
                    return new Token(TokenType.Integer, new string(digits));

                IEnumerable<char> Step()
                {
                    yield return firstDigit;

                    while (char.IsDigit((char)reader.Peek()) || (char)reader.Peek() == '.')
                    {
                        yield return (char)reader.Read();
                    }
                }
            }
        }
    }
}