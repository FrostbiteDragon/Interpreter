using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public class Parameter : IStatement
    {
        public string Id { get; }
        public IDataType Type { get; }

        public Parameter(string id, IDataType type)
        {
            Id = id;
            Type = type;
        }
    }
}
