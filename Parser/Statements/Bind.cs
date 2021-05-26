using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript.Statements
{
    public class Bind : IStatement
    {
        public string Id { get; }
        public IExpression Value { get; }

        public Bind(string id, IExpression value)
        {
            Id = id;
            Value = value;
        }

        public void Deconstruct(out string id, out IExpression value)
        {
            id = Id;
            value = Value;
        }
    }
}
