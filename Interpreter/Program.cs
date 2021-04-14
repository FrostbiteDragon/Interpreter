using System;
using System.IO;
using System.Linq;
using FrostScript;

var tokens = Lexer.Tokenize(File.ReadAllText(args[0])).ToArray();
var nodes = Parser.GenerateAST(tokens).ToArray();
Interpreter.RunAST(nodes);