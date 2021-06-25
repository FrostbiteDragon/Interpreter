using FrostScript.Statements;

namespace FrostScript.Expressions
{
    internal interface ICallableExpression
    {
        Parameter Parameter { get; }
    }
}