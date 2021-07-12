using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class While : IExpression
    {
        public IExpression Condition { get; init; }
        public IExpression[] Body { get; init; }

        public IDataType Type => DataType.Void;

        public While(IExpression condition, IExpression[] body)
        {
            Condition = condition;
            Body = body;
        }
    }
}
