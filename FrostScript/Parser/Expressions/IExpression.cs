using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public interface IExpression
    {
        public DataType Type { get; }
    }
}
