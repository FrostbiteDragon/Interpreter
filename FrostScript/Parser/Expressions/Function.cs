using FrostScript.Statements;
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
        public DataType Type => Body.Type;

        public Function(Parameter parameter, IExpression body)
        {
            Parameter = parameter;
            Body = body;
        }
    }
}
