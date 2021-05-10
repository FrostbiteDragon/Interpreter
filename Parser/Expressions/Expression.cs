using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public abstract class Expression
    {
        public DataType Type { get; init; }

        protected Expression() { }

        protected Expression(DataType type)
        {
            Type = type;
        }
    }
}
