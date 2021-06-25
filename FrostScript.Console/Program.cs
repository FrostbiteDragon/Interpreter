using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrostScript;
using FrostScript.Expressions;
using FrostScript.NativeFunctions;
using FrostScript.Nodes;
using FrostScript.Statements;
using Frostware.Result;

var tokens = Lexer.GetTokens(File.ReadAllText(args[0])).ToArray();

Dictionary<string, IExpression> nativeFunctions = new()
{
    ["print"] = new PrintFunction(),
    ["clock"] = new ClockFunction()
};

if (NodeParser.GenerateNodes(tokens) is Pass<INode> ast)
{
    if (ast.Value.ToTypedNode(nativeFunctions) is Pass<IExpression> typedNode)
        Interpreter.ExecuteExpression(typedNode.Value, nativeFunctions);
}

//Railroad.Rail<INode, string[]>(
//    NodeParser.GenerateNodes(tokens),
//    (ast) => ast.ToTypedNode(nativeFunctions),
//    (errors) => { foreach (var error in errors) Console.WriteLine(error); }
//);

//if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> program)
//    Interpreter.ExecuteProgram(program.Value, nativeFunctions);
//else
//    Console.WriteLine("Parsing failed");

