using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class FunctionNode : INode
    {
        public Token Token { get; }
        public Parameter Parameter { get; }
        public INode Body { get; }

        public FunctionNode(Token token, Parameter parameter, INode body)
        {
            Token = token;
            Parameter = parameter;
            Body = body;
        }

        public static readonly Func<ParseFunc, ParseFunc> function = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.Fun)
                return next(pos, tokens);

            var parameters = new List<Parameter>();
            var currentPos = pos + 1;
            while (tokens[currentPos].Type is not TokenType.Arrow)
            {
                var (paramater, newPos) = Parameter.parameter(currentPos, tokens);
                parameters.Add(paramater);
                currentPos = newPos;
            }

            if (tokens[currentPos].Type is not TokenType.Arrow)
                throw new ParseException(tokens[currentPos].Line, tokens[currentPos].Character, $"expected \"->\" but got {tokens[currentPos].Lexeme}", currentPos + 1);

            var (body, bodyPos) = Expression.expression(currentPos + 1, tokens);

            FunctionNode function = null;

            if (parameters.Count == 0)
            {
                function = new FunctionNode(tokens[pos], new Parameter("", DataType.Void), body);
            }
            else
            {
                foreach (var parameter in parameters.Reverse<Parameter>())
                {
                    if (function is null)
                        function = new FunctionNode(tokens[pos], parameter, body);
                    else
                    {
                        function = new FunctionNode(tokens[pos], parameter, function);
                    }
                }
            }
            return (function, bodyPos);
        };
    }
}
