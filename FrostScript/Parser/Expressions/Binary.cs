using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Binary : IExpression
    {
        public IExpression Left { get; init; }
        public Token Operator { get; init; }
        public IExpression Right { get; init; }

        public DataType Type { get; }

        public Binary(DataType type, IExpression left, Token @operator, IExpression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;

            Type = type;
        }
    }
}
