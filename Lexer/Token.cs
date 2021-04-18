using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public enum TokenType 
    { 
        Operator,
        NewLine,
        ParentheseOpen,
        ParentheseClose,
        BraceOpen,
        BraceClose,
        Comma,
        Dot,
        Assign,
        Discard,
        Arrow,

        //Oporators
        Minus, Plus, Slash, Star,

        //logical oporators
        Equal, NotEqual, GreaterThen, GreaterOrEqual, LessOrEqual, LessThen, Not,
        And, Or,

        //Literals
        Numeral, String, Bool, Id, Null,

        //Keywords
        If, Else, 
        Print,
        True, False,
        For, While,

        Eof
    }

    

    public class Token
    {
        public TokenType Type { get; init; }
        public string Lexeme { get; init; }
        public object Literal { get; init; }

        public Token(TokenType type)
        {
            Type = type;
        }

        public Token(TokenType type, string value, object literal = null)
        {
            Type = type;
            Lexeme = value;
            Literal = literal;
        }

        public bool IsHigherPrecidence(string oporator)
        {
            var result = oporator switch
            {
                "-" or "+" => false,
                _ when oporator == "*" || oporator == "/" => Lexeme == "-" || Lexeme == "+",
                _ => false
            };

            return result;
        }
    }
}
