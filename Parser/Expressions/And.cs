using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class And : Expression
    {
        public Expression Left { get; init; }
        public Expression Right { get; init; }

        public And(Expression left, Expression right) : base(DataType.Bool)
        {
            Left = left;
            Right = right;
        }
    }
}
