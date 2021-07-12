using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Function : IExpression, ICallableExpression
    {
        public Parameter Parameter { get; }
        public IExpression Body { get; }
        public IDataType Type { get; }

        public Function(Parameter parameter, IExpression body, FunctionType functionType)
        {
            Parameter = parameter;
            Body = body;
            Type = functionType;
        }
    }
}
