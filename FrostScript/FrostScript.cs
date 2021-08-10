using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using FrostScript.Expressions;
using FrostScript.NativeFunctions;
using Frostware.Result;
using static FrostScript.Lexer;
using static FrostScript.Parser;
using static FrostScript.Validator;
using static FrostScript.Interpreter;

namespace FrostScript
{
    public static class FrostScript
    {
        public static readonly IReadOnlyDictionary<string, IExpression> NativeFunctions = new Dictionary<string, IExpression>()
        {
            ["print"] = new PrintFunction(),
        };

        private static readonly Func<string, Result> execute = 
            lex
            .Fish(parse)
            .Fish(validate(new(NativeFunctions)))
            .Fish(interpret(new(NativeFunctions)));


        public static void ExecuteString(string frostScript)
        {
            execute(frostScript);
        }

        public static T ExecuteString<T>(string frostScript)
        {
            return (T)(execute(frostScript) as Pass<object>).Value;
        }

        public static void ExecuteFile(string path)
        {
            execute(File.ReadAllText(path));
        }

        public static T ExecuteFile<T>(string path)
        {
            return (T)(execute(File.ReadAllText(path)) as Pass<object>).Value;
        }
    }
}
