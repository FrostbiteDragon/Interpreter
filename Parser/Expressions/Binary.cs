using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Binary : Expression
    {
        public Expression Left { get; init; }
        public Token Operator { get; init; }
        public Expression Right { get; init; }

        public Binary(DataType type, Expression left, Token @operator, Expression right) : base(type)
        {
            Left = left;
            Operator = @operator;
            Right = right;
        }
    }
}
