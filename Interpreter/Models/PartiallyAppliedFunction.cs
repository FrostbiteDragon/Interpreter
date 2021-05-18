using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class PartiallyAppliedFunction : IExpression
    {
        public IReadOnlyDictionary<string, IExpression> Arguments { get; }
        public Function Body { get; }
        public DataType Type => Body.Type;

        public PartiallyAppliedFunction(Dictionary<string, IExpression> arguments, Function body)
        {
            Arguments = arguments;
            Body = body;
        }
    }
}
