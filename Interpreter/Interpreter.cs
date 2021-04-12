using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class Interpreter
    {
        public static void RunAST(IEnumerable<Node> nodes)
        {
            Dictionary<string, string> variables = new Dictionary<string, string>();

            foreach (Node node in nodes)
            {
                ExecuteNode(node);
                Console.WriteLine(node);
            }

            void ExecuteNode(Node node)
            {
                if (node.Token.Type == TokenType.Integer)
                    return;

                else if (node.Token.Type == TokenType.Assign)
                    ExecuteAssign(node);

                else if (node.Token.Type == TokenType.Operator)
                    ExecuteOporator(node);
                else
                    throw new Exception($"unsuported token type: {node.Token.Type}");
            }

            void ExecuteAssign(Node node)
            {
                ExecuteNode(node.Right);
                variables[node.Left.Token.value] = node.Right.Token.value;
            }

            void ExecuteOporator(Node node)
            {
                if (node.Left.Token.Type == TokenType.Integer || node.Left.Token.Type == TokenType.Id)
                {
                    if (node.Right.Token.Type == TokenType.Integer || node.Right.Token.Type == TokenType.Id)
                    {
                        node.Token = Calaps(node);
                        node.Right = null;
                        node.Left = null;
                    }
                    else if (node.Right != null)
                    {
                        ExecuteNode(node.Right);
                        ExecuteNode(node);
                    }
                }
                else if (node.Left != null)
                {
                    ExecuteNode(node.Left);
                    ExecuteNode(node);
                }
            }

            Token Calaps(Node node)
            {
                return node.Token.value switch
                {
                    "+" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) + ParseIntOrId(node.Right.Token)).ToString()),
                    "-" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) - ParseIntOrId(node.Right.Token)).ToString()),

                    "*" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) * ParseIntOrId(node.Right.Token)).ToString()),
                    "/" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) / ParseIntOrId(node.Right.Token)).ToString()),
                    _ => throw new Exception($"Did not recognise character: {node.Token.value}")
                };

                int ParseIntOrId(Token token)
                {
                    return token.Type switch
                    {
                        TokenType.Integer => int.Parse(token.value),
                        TokenType.Id => int.Parse(variables[token.value]),
                        _ => throw new Exception("Unexpected token")
                    };
                }
            }
        }
    }
}
