using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Unknown : IExpression
    {
        public DataType Type { get; }

        public Unknown(DataType type = DataType.Unknown) 
        {
            Type = type;
        }
    }
}
