using System.Collections.Generic;


namespace Interpreter
{
    public static class Parser
    {
        public static IEnumerable<Node> GenerateAST(Token[] tokens)
        {
            yield return GenerateNode(new Node(tokens[0]), tokens, 0);

            Node GenerateNode(Node node, Token[] tokens, int pos)
            {
                if (pos >= tokens.Length - 1)
                    return node;

                if (tokens[pos].Type != TokenType.Operator)
                    return GenerateNode(node, tokens, pos + 1);
                else
                {
                    var newNode = new Node(tokens[pos], node, new(tokens[pos + 1]));

                    return GenerateNode(newNode, tokens, pos + 1);
                }
            }
        }
    }
}
