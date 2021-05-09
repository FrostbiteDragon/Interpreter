using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Literal : Expression
    {
        public object Value { get; init; }

        public Literal(object value)
        {
            Value = value;
        }
    }
}
