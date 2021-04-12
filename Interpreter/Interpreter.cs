using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class Interpreter
    {
        public static void CalapseAST(IEnumerable<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                ExecuteNode(node);
                Console.WriteLine(node.Token.value);
            }

            void ExecuteNode(Node node)
            {
                if (node.Token.Type == TokenType.Integer)
                    return;
                if (node.Token.Type == TokenType.Operator)
                {
                    if (node.Left.Token.Type == TokenType.Integer)
                    {
                        if (node.Right.Token.Type == TokenType.Integer)
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
                else
                    throw new Exception($"unsuported token type: {node.Token.Type}");
            }

            Token Calaps(Node node)
            {
                return node.Token.value switch
                {
                    "+" => new Token(TokenType.Integer, (int.Parse(node.Left.Token.value) + int.Parse(node.Right.Token.value)).ToString()),
                    "-" => new Token(TokenType.Integer, (int.Parse(node.Left.Token.value) - int.Parse(node.Right.Token.value)).ToString()),

                    "*" => new Token(TokenType.Integer, (int.Parse(node.Left.Token.value) * int.Parse(node.Right.Token.value)).ToString()),
                    "/" => new Token(TokenType.Integer, (int.Parse(node.Left.Token.value) / int.Parse(node.Right.Token.value)).ToString()),
                    _ => throw new Exception($"Did not recognise character: {node.Token.value}")
                };
            }
        }
    }
}
