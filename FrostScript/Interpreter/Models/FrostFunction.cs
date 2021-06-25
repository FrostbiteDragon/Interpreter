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
        public Parameter Parameter => Function.Parameter;
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

        public override string ToString()
        {
            var funString = $"fun ";

            void AddParameter(Function function)
            {
                funString += $"{function.Parameter.Id}:{function.Parameter.Type} -> ";

                if (function.Body is Function body)
                    AddParameter(body);
            }

            AddParameter(Function);

            funString += $"{Type}";

            return funString;
        }
    }
}
