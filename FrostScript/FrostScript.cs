using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrostScript.Expressions;
using FrostScript.NativeFunctions;
using FrostScript.Statements;
using Frostware.Result;

namespace FrostScript
{
    public static class FrostScript
    {
        public static readonly IReadOnlyDictionary<string, IExpression> NativeFunctions = new Dictionary<string, IExpression>()
        {
            ["print"] = new PrintFunction(),
            ["clock"] = new ClockFunction()
        };

        public static void ExecuteString(string frostScript, Dictionary<string, IExpression> nativeFunctions = null)
        {
            if (nativeFunctions is null)
                nativeFunctions = new(NativeFunctions);
            else
                nativeFunctions = nativeFunctions.Union(NativeFunctions).ToDictionary(x => x.Key, x => x.Value);

            var tokens = Lexer.GetTokens(frostScript).ToArray();

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Fail)
                    throw new Exception("Interpretation Failed");
            }
            else throw new Exception("Parsing failed");
        }

        public static T ExecuteString<T>(string frostScript, Dictionary<string, IExpression> nativeFunctions = null)
        {
            if (nativeFunctions is null)
                nativeFunctions = new(NativeFunctions);
            else
                nativeFunctions = nativeFunctions.Union(NativeFunctions).ToDictionary(x => x.Key, x => x.Value);

            var tokens = Lexer.GetTokens(frostScript).ToArray();

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Pass<object> programResult)
                    return (T)programResult.Value;
                else throw new Exception("Interpretation failed");
            }
            else throw new Exception("Parsing failed");
        }

        public static void ExecuteFile(string path, Dictionary<string, IExpression> nativeFunctions = null)
        {
            if (nativeFunctions is null)
                nativeFunctions = new Dictionary<string, IExpression>();

            var tokens = Lexer.GetTokens(File.ReadAllText(path)).ToArray();

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Fail)
                    throw new Exception("Interpretation failed");
            }
            else throw new Exception("Parsing failed");
        }

        public static T ExecuteFile<T>(string path, Dictionary<string, IExpression> nativeFunctions = null)
        {
            if (nativeFunctions is null)
                nativeFunctions = new(NativeFunctions);
            else
                nativeFunctions = nativeFunctions.Union(NativeFunctions).ToDictionary(x => x.Key, x => x.Value);

            var tokens = Lexer.GetTokens(File.ReadAllText(path)).ToArray();

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Pass<object> programResult)
                    return (T)programResult.Value;
                else throw new Exception("Interpretation failed");
            }
            else throw new Exception("Parsing failed");
        }
    }
}
