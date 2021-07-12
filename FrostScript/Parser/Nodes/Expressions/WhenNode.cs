using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Nodes
{
    public class WhenNode : INode
    {
        public Token Token { get; }
        public (INode boolExpression, INode resultExpression)[] Clauses { get; }

        public WhenNode(Token token, (INode boolExpression, INode resultExpression)[] clauses)
        {
            Token = token;
            Clauses = clauses;
        }

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> when = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.When)
                return next(pos, tokens);

            if (tokens[pos + 1].Type is not TokenType.BraceOpen)
                throw new ParseException(tokens[pos + 1].Line, tokens[pos + 1].Character, "expected '{' after \"if\"", pos + 1);

            var currentPos = pos + 1;
            IEnumerable<(INode, INode)> GetClauses(Token[] tokens)
            {
                do
                {
                    currentPos++;

                    if (tokens[currentPos].Type is TokenType.BraceClose)
                        break;

                    if (tokens[currentPos].Type is TokenType.Arrow)
                    {
                        var (defaultNode, defaultPos) = Parser.Expression(currentPos + 1, tokens);
                        yield return (new LiteralNode(new(TokenType.Void)), defaultNode);
                        currentPos = defaultPos;
                        break;
                    }

                    var (boolNode, boolPos) = Parser.Expression(currentPos, tokens);
                    if (tokens[boolPos].Type is not TokenType.Arrow)
                        throw new ParseException(tokens[boolPos].Line, tokens[currentPos].Character, $"expected \"->\"", currentPos);

                    var (resultNode, resultPos) = Parser.Expression(boolPos + 1, tokens);

                    currentPos = resultPos;
                    yield return (boolNode, resultNode);

                } while (tokens[currentPos].Type is TokenType.Comma or TokenType.Arrow);
            }

            var clauses = GetClauses(tokens).ToArray();

            if (tokens[currentPos].Type is not TokenType.BraceClose)
                throw new ParseException(tokens[currentPos].Line, tokens[currentPos].Character, "Expected '}'. if not closed", currentPos);

            return (new WhenNode(tokens[pos], clauses), currentPos + 1);
        };

        public static readonly Func<Func<int, Token[], (INode node, int pos)>, Func<int, Token[], (INode node, int pos)>> @if = (next) => (pos, tokens) =>
        {
            if (tokens[pos].Type is not TokenType.If)
                return next(pos, tokens);

            var currentPos = pos + 1;
            IEnumerable<(INode, INode)> GetClauses(Token[] tokens)
            {
                var (boolNode, boolPos) = Parser.Expression(currentPos, tokens);

                if (tokens[boolPos].Type is not TokenType.Arrow)
                    throw new ParseException(tokens[boolPos].Line, tokens[currentPos].Character, $"expected \"->\"", currentPos);

                var (resultNode, resultPos) = Parser.Expression(boolPos + 1, tokens);

                yield return (boolNode, resultNode);

                currentPos = resultPos;

                while (tokens[currentPos].Type is TokenType.Else)
                {
                    var (elseBoolNode, elseBoolPos) = Parser.Expression(currentPos + 1, tokens);

                    if (tokens[elseBoolPos].Type is not TokenType.Arrow)
                    {
                        yield return (new LiteralNode(new(TokenType.Void)), elseBoolNode);
                        currentPos = elseBoolPos;
                        break;
                    }
                        //throw new ParseException(tokens[elseBoolPos].Line, tokens[elseBoolPos].Character, $"expected \"->\"", elseBoolPos);

                    var (elseResultNode, elseResultPos) = Parser.Expression(elseBoolPos + 1, tokens);

                    currentPos = elseResultPos;
                    yield return (elseBoolNode, elseResultNode);
                } 
            }

            var clauses = GetClauses(tokens).ToArray();

            return (new WhenNode(tokens[pos], clauses), currentPos);
        };
    }

}
