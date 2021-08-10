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

        public IDataType Type { get; }

        public Call(IExpression callee, IExpression argument, IDataType type)
        {
            Callee = callee;
            Argument = argument;
            Type = type;
        }
    }
}
