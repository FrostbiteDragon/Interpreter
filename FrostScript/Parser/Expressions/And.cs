using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class And : IExpression
    {
        public IExpression Left { get; init; }
        public IExpression Right { get; init; }

        public DataType Type => DataType.Bool;

        public And(IExpression left, IExpression right)
        {
            Left = left;
            Right = right;
        }
    }
}
