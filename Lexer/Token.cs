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
        public Token(TokenType type, string value)
        {
            Type = type;
            this.value = value;
        }

        public TokenType Type { get; init; }
        public string value { get; init; }
    }
}
