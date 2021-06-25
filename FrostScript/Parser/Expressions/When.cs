using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class When : IExpression
    {
        public IExpression IfExpresion { get; }
        public IExpression ResultExpression { get; }
        public When ElseWhen { get; }

        public DataType Type => ResultExpression.Type;

        public When(IExpression ifExpresion, IExpression resultExpression, When elseWhen)
        {
            IfExpresion = ifExpresion;
            ResultExpression = resultExpression;
            ElseWhen = elseWhen;
        }
    }
}
