using FrostScript.Statements;
using System.Collections.Generic;

namespace FrostScript.Expressions
{
    public interface ICallable : IExpression, Statements.IStatement
    {
        public object Call(object argument);
    }
}