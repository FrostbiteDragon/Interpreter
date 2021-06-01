
using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class If : IStatement
    {
        public IExpression IfExpresion { get; init; }
        public IStatement ResultStatement { get; init; }
        public If ElseIf { get; set; }

        public If() { }

        public If(IExpression ifExpresion, IStatement resultStatement, If elseif)
        {
            IfExpresion = ifExpresion;
            ResultStatement = resultStatement;
            ElseIf = elseif;
        }
    }
}
