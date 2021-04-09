using System;
using System.IO;
using System.Linq;
using Interpreter;

var tokens = Lexer.Tokenize(File.ReadAllText(args[0])).ToArray();
var nodes = Parser.GenerateAST(tokens);
Interpreter.Interpreter.CalapseAST(nodes);