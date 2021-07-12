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
        SemiColon,
        ParentheseOpen,
        ParentheseClose,
        BraceOpen,
        BraceClose,
        Comma,
        Dot,
        Assign,
        Discard,
        Arrow,
        Pipe,
        ClosePipe,
        ReturnPipe,
        Colon,
        Eof,

        //Oporators
        Minus, Plus, Slash, Star,

        //logical oporators
        Equal, NotEqual, GreaterThen, GreaterOrEqual, LessOrEqual, LessThen, Not,
        And, Or,

        //functional oporators
        PipeOp,

        //Literals
        Numeral, Int, Double, String, Bool, Id, Void,

        //Keywords
        If, Else, When, 
        Print,
        True, False,
        For, To, DownTo, While,
        Var, Let,
        Fun,
        Increment, Decrement,

        //types
        IntType,
        DoubleType,
        BoolType,
        StringType
    }

    

    public class Token
    {
        public TokenType Type { get; init; }
        public string Lexeme { get; init; }
        public object Literal { get; init; }
        public int Line { get; init; }
        public int Character { get; init; }

        public Token(TokenType type)
        {
            Type = type;
        }

        public Token(TokenType type, int line, int character)
        {
            Type = type;
            Line = line;
            Character = character;
        }

        public Token(TokenType type, int line, int character, string lexeme, object literal = null)
        {
            Type = type;
            Line = line;
            Character = character; 
            Lexeme = lexeme;
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
