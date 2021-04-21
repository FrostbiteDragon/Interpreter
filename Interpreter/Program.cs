using System;
using System.IO;
using System.Linq;
using FrostScript;

var tokens = Lexer.GetTokens(File.ReadAllText(args[0])).ToArray();
var Expression = Parser.GenerateAST(tokens);

try { Console.WriteLine(Interpreter.ExecuteExpression(Expression)); }
catch(InterpretException exception)
{
    Reporter.Report(exception.Line, exception.CharacterPos, exception.Message);
}