using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrostScript;
using FrostScript.Statements;
using Frostware.Result;

var tokens = Lexer.GetTokens(File.ReadAllText(args[0])).ToArray();

switch (Parser.GetAST(tokens))
{
    case Pass<IStatement[]> pass : Interpreter.ExecuteProgram(pass.Value); break;

}



