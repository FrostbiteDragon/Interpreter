using FrostScript.Expressions;
using System.Collections.Generic;

namespace FrostScript.Expressions
{
    public interface ICallable : IExpression
    {
        public object Call(object argument);
    }
}