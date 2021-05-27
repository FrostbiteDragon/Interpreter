using FrostScript.Expressions;
using FrostScript.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class FrostFunction : ICallable
    {
        public Function Function { get; }
        public DataType Type => Function.Type;
        private readonly Dictionary<string, IExpression> closure;

        public FrostFunction(Function function, Dictionary<string, IExpression> closure)
        {
            Function = function;
            this.closure = closure;
        }

        public object Call(object argument)
        {
            if (argument is not null)
                closure[Function.Parameter.Id] = new Literal(Function.Parameter.Type, argument);

            return Interpreter.ExecuteExpression(Function.Body, closure);
        }
    }
}
