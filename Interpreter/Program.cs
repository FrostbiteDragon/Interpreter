using System;
using System.IO;
using System.Linq;
using FrostScript;

var tokens = Lexer.GetTokens(File.ReadAllText(args[0])).ToArray();
var nodes = Parser.GenerateAST(tokens).ToArray();
Console.Read();
//Interpreter.RunAST(nodes);