using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class WhenNode : INode
    {
        public Token Token { get; set; }
        public INode IfExpresion { get; init; }
        public INode ResultExpression { get; init; }
        public WhenNode ElseWhen { get; set; }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> when = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.When)
                return next(pos, tokens);

            if (tokens[pos + 1].Type is not TokenType.BraceOpen)
                throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, "expected '{' after when", pos + 1);

            var (when, newPos) = GenerateWhen(null, pos + 2, tokens);

            if (tokens[newPos].Type is not TokenType.BraceClose)
                throw new ParseException(tokens[newPos].Line, tokens[newPos].Character, "expected '}'. \"when\" was never closed", newPos);

            return (when, newPos + 1);

            static (WhenNode when, int pos) GenerateWhen(WhenNode when, int pos, Token[] tokens)
            {
                if (pos >= tokens.Length)
                    throw new ParseException(tokens.Last().Line, tokens.Last().Character, $"Missing default clause", pos);

                //default clause
                if (tokens[pos].Type is TokenType.Arrow)
                {
                    var (expression, newPos) = NodeParser.Expression(pos + 1, tokens);

                    var newWhen = when switch
                    {
                        null => new WhenNode
                        {
                            ResultExpression = expression
                        },
                        _ => new WhenNode
                        {
                            IfExpresion = when.IfExpresion,
                            ResultExpression = when.ResultExpression,
                            ElseWhen = new WhenNode { ResultExpression = expression }
                        }
                    };
                    return (newWhen, newPos);

                }
                //if clause
                else
                {
                    var boolResult = NodeParser.Expression(pos, tokens);

                    if (tokens[boolResult.pos].Type is not TokenType.Arrow)
                        throw new ParseException(tokens[pos].Line, tokens[pos].Character, $"expected \"->\"", pos);

                    var (expression, newPos) = NodeParser.Expression(boolResult.pos + 1, tokens);
                    var newWhen = new WhenNode
                    {
                        IfExpresion = boolResult.node,
                        ResultExpression = expression
                    };

                    if (tokens[newPos].Type is not TokenType.Comma)
                        throw new ParseException(
                            tokens[newPos].Line,
                            tokens[newPos].Character,
                            $"expected \',\'",
                            newPos + tokens.Skip(newPos).TakeWhile(x => x.Type is not TokenType.BraceClose).Count() + 1);

                    if (when is null)
                        return GenerateWhen(newWhen, newPos + 1, tokens);
                    else
                    {
                        var result = GenerateWhen(newWhen, newPos + 1, tokens);

                        return (new WhenNode
                        {
                            IfExpresion = when.IfExpresion,
                            ResultExpression = when.ResultExpression,
                            ElseWhen = result.when
                        }, result.pos);
                    }
                }
            }
        };
    }
}
