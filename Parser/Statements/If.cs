
using FrostScript.Expressions;

namespace FrostScript.Statements
{
    public class If : Statement
    {
        public Expression IfExpresion { get; init; }
        public Statement ResultStatement { get; init; }
        public If ElseIf { get; set; }

        public If() { }

        public If(Expression ifExpresion, Statement resultStatement, If elseif)
        {
            IfExpresion = ifExpresion;
            ResultStatement = resultStatement;
            ElseIf = elseif;
        }
    }
}
