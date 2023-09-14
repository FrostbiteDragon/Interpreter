﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class BindNode : INode
    {
        public Token Token { get; }
        public string Id { get; }
        public INode Value { get; }
        public bool Mutable { get; }

        public BindNode(Token token, string id, INode value, bool mutable)
        {
            Token = token;
            Id = id;
            Value = value;
            Mutable = mutable;
        }

        public static readonly Func<ParseFunc, ParseFunc> bind = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is not (TokenType.Let or TokenType.Var))
                return next(pos, tokens);

            var mutability = tokens[pos].Type is TokenType.Var;

            var id = tokens[pos + 1].Lexeme;

            if (tokens[pos + 2].Type is not TokenType.Assign)
                throw new ParseException(tokens[pos + 2], $"expected '=' but recieved \"{tokens[pos + 2].Lexeme}\"", pos + 2);

            var (value, newPos) = Expression.expression(pos + 3, tokens);

            return (new BindNode(tokens[pos], id, value, mutability), newPos);
        };
    }
}
