using System;
using System.Collections.Generic;

namespace FrostScript
{
    public static class Interpreter
    {
        static readonly Dictionary<string, string> variables = new Dictionary<string, string>();
        public static void RunAST(IEnumerable<Node> nodes)
        {

            //foreach (Node node in nodes)
            //{
            //    ExecuteNode(node);
            //}

        //    void ExecuteNode(Node node)
        //    {
        //        if (node.Token.Type == TokenType.Integer)
        //            return;

        //        if (node.Token.Type == TokenType.Id)
        //        {
        //            node.Token = new(TokenType.Integer, variables[node.Token.Lexeme]);
        //            return;
        //        }

        //        else if (node.Token.Type == TokenType.Assign)
        //            ExecuteAssign(node);

        //        else if (node.Token.Type == TokenType.Print)
        //            ExecutePrint(node);

        //        else if (node.Token.Type == TokenType.Operator)
        //            ExecuteOporator(node);
        //        else
        //            throw new NotImplementedException($"no method to execute token type given: {node.Token.Type}");
        //    }

        //    void ExecuteAssign(Node node)
        //    {
        //        var variableId = node.Left.Token.Lexeme;

        //        ExecuteNode(node.Right);
        //        variables[variableId] = node.Right.Token.Lexeme;
        //    }

        //    void ExecutePrint(Node node)
        //    {
        //        ExecuteNode(node.Right);
        //        Console.WriteLine(node.Right.Token.Lexeme);
        //    }

        //    void ExecuteOporator(Node node)
        //    {
        //        if (node.Left.Token.Type == TokenType.Integer || node.Left.Token.Type == TokenType.Id)
        //        {
        //            if (node.Right.Token.Type == TokenType.Integer || node.Right.Token.Type == TokenType.Id)
        //            {
        //                node.Token = Calaps(node);
        //                node.Right = null;
        //                node.Left = null;
        //            }
        //            else if (node.Right != null)
        //            {
        //                ExecuteNode(node.Right);
        //                ExecuteNode(node);
        //            }
        //        }
        //        else if (node.Left != null)
        //        {
        //            ExecuteNode(node.Left);
        //            ExecuteNode(node);
        //        }
        //    }

        //    Token Calaps(Node node)
        //    {
        //        return node.Token.Lexeme switch
        //        {
        //            "+" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) + ParseIntOrId(node.Right.Token)).ToString()),
        //            "-" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) - ParseIntOrId(node.Right.Token)).ToString()),

        //            "*" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) * ParseIntOrId(node.Right.Token)).ToString()),
        //            "/" => new Token(TokenType.Integer, (ParseIntOrId(node.Left.Token) / ParseIntOrId(node.Right.Token)).ToString()),
        //            _ => throw new Exception($"Did not recognise character: {node.Token.Lexeme}")
        //        };

        //        int ParseIntOrId(Token token)
        //        {
        //            return token.Type switch
        //            {
        //                TokenType.Integer => int.Parse(token.Lexeme),
        //                TokenType.Id => int.Parse(variables[token.Lexeme]),
        //                _ => throw new Exception("Unexpected token")
        //            };
        //        }
        //    }
        }
    }
}
