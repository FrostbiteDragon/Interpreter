using FrostScript.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Yield : IExpression
    {
        public IExpression Value { get; }
        public IDataType Type => Value.Type;

        public Yield(IExpression value)
        {
            Value = value;
        }
    }
}
