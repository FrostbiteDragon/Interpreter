using FrostScript.DataTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class Loop : IExpression
    {
        public Bind Bind { get; }
        public IExpression Condition { get; }
        public Assign Assign { get; }
        public IExpression[] Body { get; }
        public IDataType Type => DataType.Void;

        public Loop(Bind bind, IExpression condition, Assign assign, IExpression[] body)
        {
            Bind = bind;
            Condition = condition;
            Assign = assign;
            Body = body;
        }
    }
}
