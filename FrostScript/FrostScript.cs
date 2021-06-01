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
        public static void ExecuteString(string frostScript)
        {
            var tokens = Lexer.GetTokens(frostScript).ToArray();

            Dictionary<string, IExpression> nativeFunctions = new()
            {
                ["print"] = new PrintFunction(),
                ["clock"] = new ClockFunction()
            };

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Fail)
                    throw new Exception("Interpretation Failed");
            }
            else throw new Exception("Parsing failed");
        }

        public static T ExecuteString<T>(string frostScript)
        {
            var tokens = Lexer.GetTokens(frostScript).ToArray();

            Dictionary<string, IExpression> nativeFunctions = new()
            {
                ["print"] = new PrintFunction(),
                ["clock"] = new ClockFunction()
            };

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Pass<object> programResult)
                    return (T)programResult.Value;
                else throw new Exception("Interpretation failed");
            }
            else throw new Exception("Parsing failed");
        }

        public static void ExecuteFile(string path)
        {
            var tokens = Lexer.GetTokens(File.ReadAllText(path)).ToArray();

            Dictionary<string, IExpression> nativeFunctions = new()
            {
                ["print"] = new PrintFunction(),
                ["clock"] = new ClockFunction()
            };

            if (Parser.GetAST(tokens, nativeFunctions) is Pass<IStatement[]> ast)
            {
                if (Interpreter.ExecuteProgram(ast.Value, nativeFunctions) is Fail)
                    throw new Exception("Interpretation failed");
            }
            else throw new Exception("Parsing failed");
        }

        public static T ExecuteFile<T>(string path)
        {
            var tokens = Lexer.GetTokens(File.ReadAllText(path)).ToArray();

            Dictionary<string, IExpression> nativeFunctions = new()
            {
                ["print"] = new PrintFunction(),
                ["clock"] = new ClockFunction()
            };

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
