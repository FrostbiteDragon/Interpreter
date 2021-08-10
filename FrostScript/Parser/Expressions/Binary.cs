using FrostScript.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Binary : IExpression
    {
        public IExpression Left { get; }
        public Token Operator { get; }
        public IExpression Right { get; }

        public IDataType Type { get; }

        public Binary() { }

        public Binary(IDataType type, IExpression left, Token @operator, IExpression right)
        {
            Left = left;
            Operator = @operator;
            Right = right;

            Type = type;
        }
    }
}
