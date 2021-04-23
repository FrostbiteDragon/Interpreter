using System;
using System.IO;
using System.Linq;
using FrostScript;

var tokens = Lexer.GetTokens(File.ReadAllText(args[0])).ToArray();
var program = Parser.GenerateAST(tokens).ToArray();

Interpreter.ExecuteProgram(program);

