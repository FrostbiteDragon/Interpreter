using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrostScript;
using FrostScript.Expressions;
using FrostScript.NativeFunctions;
using FrostScript.Statements;
using Frostware.Result;

var tokens = Lexer.GetTokens(File.ReadAllText(args[0])).ToArray();

Dictionary<string, IExpression> nativeFunctions = new()
{
    ["print"] = new PrintFunction(),
    ["clock"] = new ClockFunction()
};

if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> program)
    Interpreter.ExecuteProgram(program.Value, nativeFunctions);