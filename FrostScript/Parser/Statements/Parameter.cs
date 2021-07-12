using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Statements
{
    public struct Parameter : IStatement
    {
        public string Id { get; }
        public IDataType Type { get; }

        public Parameter(string id, IDataType type)
        {
            Id = id;
            Type = type;
        }

        public static readonly Func<int, Token[], (Parameter parameter, int pos)> parameter = (pos, tokens) =>
        {
            var id = tokens[pos].Type switch
            {
                TokenType.Id => tokens[pos].Lexeme,
                _ => throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected Id", pos + 3)
            };

            if (tokens[pos + 1].Type is not TokenType.Colon)
                throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"Expected ':' but got {tokens[pos].Lexeme}", pos + 3);

            IDataType type = tokens[pos + 2].Type switch
            {
                TokenType.IntType => DataType.Int,
                TokenType.DoubleType => DataType.Double,
                TokenType.StringType => DataType.String,
                TokenType.BoolType => DataType.Bool,
                _ => throw new ParseException(tokens[pos + 2].Line, tokens[pos + 2].Character, $"Expected type but got {tokens[pos + 2].Lexeme}", pos + 3)
            };

            return (new Parameter(id, type), pos + 3);
        };
    }
}
