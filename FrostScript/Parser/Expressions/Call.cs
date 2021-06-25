using FrostScript.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Call : IExpression
    {
        public IExpression Callee { get; }
        public IExpression Argument { get; }

        public IDataType Type => Callee.Type;

        public Call(IExpression callee, IExpression argument = null)
        {
            Callee = callee;
            Argument = argument;
        }
    }
}
