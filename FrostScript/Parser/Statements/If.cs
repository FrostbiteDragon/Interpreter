
using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class If : IStatement
    {
        public IStatement IfExpresion { get; init; }
        public IStatement ResultStatement { get; init; }
        public If ElseIf { get; set; }

        public If() { }

        public If(IStatement ifExpresion, IStatement resultStatement, If elseif)
        {
            IfExpresion = ifExpresion;
            ResultStatement = resultStatement;
            ElseIf = elseif;
        }
    }
}
