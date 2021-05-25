using FrostScript.Statements;
using System.Collections.Generic;

namespace FrostScript.Expressions
{
    public interface ICallable : IExpression, IStatement
    {
        public object Call(object argument);
    }
}