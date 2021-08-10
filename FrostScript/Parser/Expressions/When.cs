using FrostScript.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class When : IExpression
    {
        public (IExpression BoolExpression, IExpression ResultExpression)[] Clauses { get; }

        public IDataType Type => Clauses.Last().ResultExpression.Type;

        public When((IExpression BoolExpression, IExpression ResultExpression)[] clauses)
        {
            Clauses = clauses;
        }
    }
}
