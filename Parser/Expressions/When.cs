using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class When : Expression
    {
        public Expression IfExpresion { get; init; }
        public Expression ResultExpression { get; init; }
        public When ElseWhen { get; set; }

        public When() { }

        public When(DataType type, Expression ifExpresion, Expression resultExpression, When elseWhen) : base(type)
        {
            IfExpresion = ifExpresion;
            ResultExpression = resultExpression;
            ElseWhen = elseWhen;
        }
    }
}
