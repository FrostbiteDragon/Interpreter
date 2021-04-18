using System;
using System.Collections.Generic;
using System.Linq;

namespace FrostScript
{
    public static class Parser
    {
        public static IEnumerable<Node> GenerateAST(Token[] programeTokens)
        {
            yield return null;

            //static IEnumerable<Token[]> SplitTokens(IEnumerable<Token> tokens)
            //{
            //    var line = new List<Token>();

            //    foreach (var token in tokens)
            //    {
            //        if (token.Type == TokenType.NewLine)
            //        {
            //            yield return line.ToArray();
            //            line = new List<Token>();
            //        }
            //        else if (token.Type == TokenType.Discard)
            //            continue;

            //        else line.Add(token);
            //    }

            //    //if line did not end with a new line character
            //    if (line.Count != 0)
            //        yield return line.ToArray();
            //}

            //foreach (var tokens in SplitTokens(programeTokens))
            //{
            //    var newTokens = tokens.ToList();
            //    //add a multiplication wherever a parenthese is proceded by an interger or another parenthese
            //    for (int i = 1; i < newTokens.Count; i++)
            //    {
            //        if (newTokens[i].Type == TokenType.ParentheseOpen && (newTokens[i - 1].Type == TokenType.Integer || newTokens[i - 1].Type == TokenType.ParentheseClose))
            //        {
            //            newTokens.Insert(i, new Token(TokenType.Operator, "*"));
            //            i++;
            //        }
            //    }
                
            //    var node = GenerateNode(null, newTokens.ToArray(), 0);

            //    if (node is not null)
            //        yield return node;

            //    static Node GenerateNode(Node node, Token[] tokens, int pos)
            //    {
            //        if (pos > tokens.Length - 1)
            //            return node;

            //        return tokens[pos].Type switch
            //        {
            //            TokenType.ParentheseOpen => GenerateParenthese(node, tokens, pos),
            //            TokenType.Operator => GenerateOporator(node, tokens, pos),
            //            //solo integer (no oporator)
            //            TokenType.Integer when tokens.Length == 1 => GenerateNode(new(tokens[pos]), tokens, pos + 1),
            //            TokenType.Integer => GenerateNode(node, tokens, pos + 1),

            //            TokenType.Id when tokens.Length == 1 => GenerateNode(new(tokens[pos]), tokens, pos + 1),
            //            TokenType.Id => GenerateNode(node, tokens, pos + 1),
            //            TokenType.Print => GeneratePrintNode(node, tokens, pos),
            //            TokenType.Assign => GenerateAssignNode(node, tokens, pos),

            //            _ => throw new NotImplementedException($"{tokens[pos].Type}")
            //        };
            //    }

            //    static Node GeneratePrintNode(Node node, Token[] tokens, int pos)
            //    {
            //        return new Node(tokens[pos], null, GenerateNode(node, tokens.Skip(pos + 1).ToArray(), 0));
            //    }

            //    static Node GenerateAssignNode(Node node, Token[] tokens, int pos)
            //    {
            //        return new Node(tokens[pos], new(tokens[pos - 1]), GenerateNode(node, tokens.Skip(pos + 1).ToArray(), 0));
            //    }

            //    static Node GenerateOporator(Node node, Token[] tokens, int pos)
            //    {
            //        if (node is null)
            //        {
            //            if (tokens[pos + 1].Type == TokenType.ParentheseOpen)
            //                return GenerateParenthese(node, tokens, pos + 1);
            //            else
            //            {
            //                var newNode = new Node(tokens[pos], new(tokens[pos - 1]), new(tokens[pos + 1]));
            //                return GenerateNode(newNode, tokens, pos + 1);

            //            }
            //        }
            //        //higher precidence oporator
            //        else if (!node.IsParentese && node.Token.IsHigherPrecidence(tokens[pos].Lexeme))
            //        {
            //            if (tokens[pos + 1].Type == TokenType.ParentheseOpen)
            //                return GenerateParenthese(node, tokens, pos + 1);
            //            else
            //            {
            //                var newNode = new Node(tokens[pos], node.Right, new(tokens[pos + 1]));

            //                node.Right = newNode;

            //                return GenerateNode(node, tokens, pos + 1);
            //            }

            //        }
            //        //equal or lower precedence oporator
            //        else
            //        {
            //            if (tokens[pos + 1].Type == TokenType.ParentheseOpen)
            //                return GenerateParenthese(node, tokens, pos + 1);

            //            var newNode = new Node(tokens[pos], node, new(tokens[pos + 1]));

            //            return GenerateNode(newNode, tokens, pos + 1);
            //        }
            //    }

            //    static Node GenerateParenthese(Node node, Token[] tokens, int pos)
            //    {
            //        static IEnumerable<Token> GetParentheseTokens(Token[] tokens, int pos)
            //        {
            //            var openParenCount = 1;
            //            //we want to include all nested parentheses
            //            foreach (var token in tokens.Skip(pos + 1))
            //            {
            //                if (token.Type == TokenType.ParentheseOpen)
            //                {
            //                    openParenCount += 1;
            //                    yield return token;
            //                }

            //                else if (token.Type == TokenType.ParentheseClose)
            //                {
            //                    openParenCount -= 1;

            //                    if (openParenCount > 0)
            //                        yield return token;
            //                    else break; //break loop, we found end of parenthese

            //                }
            //                else yield return token;
            //            }
            //        }

            //        var parentheseTokens = GetParentheseTokens(tokens, pos).ToArray();

            //        var parenteseNode = GenerateNode(null, parentheseTokens, 0);
            //        //if the parentese node is not an oporator(solo int); Do not mark it as a parentese node 
            //        parenteseNode.IsParentese = parenteseNode.Token.Type == TokenType.Operator;

            //        // + 2 because, skip '(' and then skip to the next token
            //        var newPos = pos + parentheseTokens.Length + 2;

            //        if (node is null)
            //        {
            //            if (pos != 0)
            //                return GenerateNode(new Node(tokens[pos - 1], new(tokens[pos - 2]), parenteseNode), tokens, newPos);
            //            else
            //                return GenerateNode(parenteseNode, tokens, newPos);
            //        }
            //        else
            //        {
            //             //higher precidence oporator
            //            if (!node.IsParentese && node.Token.IsHigherPrecidence(tokens[pos - 1].Lexeme))
            //            {
            //                var newNode = new Node(tokens[pos - 1], node.Right, parenteseNode);

            //                node.Right = newNode;

            //                return GenerateNode(node, tokens, newPos);
            //            }
            //            //equal or lower precedence oporator
            //            else
            //            {
            //                var newNode = new Node(tokens[pos - 1], node, parenteseNode);

            //                return GenerateNode(newNode, tokens, newPos);
            //            }
            //        }
            //    }
            //}
        }
    }
}
