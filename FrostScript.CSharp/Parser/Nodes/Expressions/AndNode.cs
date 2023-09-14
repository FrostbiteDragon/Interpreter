﻿using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    internal class AndNode : INode
    {
        public INode Left { get; }
        public INode Right { get; }

        public Token Token { get; }

        public AndNode(INode left, INode right, Token token)
        {
            Left = left;
            Right = right;
            Token = token;
        }

        public static readonly Func<ParseFunc, ParseFunc> and = (next) => (pos, tokens) =>
        {
            var (node, newPos) = next(pos, tokens);

            while (newPos < tokens.Length && tokens[newPos].Type is TokenType.And)
            {
                var result = next(newPos + 1, tokens);

                node = new AndNode(node, result.node, tokens[newPos]);
                newPos = result.pos;
            }

            return (node, newPos);
        };
    }
}
