using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public enum TokenType { Integer, Operator, NewLine}

    public class Token
    {
        public TokenType Type { get; init; }
        public string value { get; init; }

        public Token(TokenType type, string value)
        {
            Type = type;
            this.value = value;
        }

        public bool IsLowerValue(string oporator)
        {
            return oporator switch
            {
                "-" or "+" => false,
                _ when oporator == "*" || oporator == "/" => value == "-" || value == "+" ? true : false,
                _ => false
            };
        }
    }
}
