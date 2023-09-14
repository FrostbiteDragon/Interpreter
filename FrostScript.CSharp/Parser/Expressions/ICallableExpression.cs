﻿using FrostScript.Expressions;

namespace FrostScript.Expressions
{
    public interface ICallableExpression : IExpression
    {
        public Parameter Parameter { get; }
        public IExpression Body { get; }
    }
}