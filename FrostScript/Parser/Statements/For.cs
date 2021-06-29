using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public enum Crement {Increment, Decrement};

    public class For : IStatement
    {
        public Bind Bind { get; }
        public IStatement EndExpression { get; }
        public Crement Crement { get; }
        public IStatement[] Body { get; }

        public For(Bind bind, IStatement endExpression, Crement crement, IEnumerable<IStatement> body)
        {
            Bind = bind;
            EndExpression = endExpression;
            Crement = crement;
            Body = body.ToArray();
        }
    }
}
