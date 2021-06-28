using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript.Statements
{
    public class Bind : IExpression
    {
        public string Id { get; }
        public IExpression Value { get; }
        public bool Mutable { get; }

        public IDataType Type => DataType.Void;

        public Bind(string id, IExpression value, bool mutable = false)
        {
            Id = id;
            Value = value;
            Mutable = mutable;
        }

        public void Deconstruct(out string id, out IExpression value)
        {
            id = Id;
            value = Value;
        }
    }
}
