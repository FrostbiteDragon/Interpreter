using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class Railroad
    {
        public static void Rail<TSuccsess, TFailure>(Result result, Action<TSuccsess> success, Action<TFailure> failure)
        {
            if (result is Pass<TSuccsess> pass) success(pass.Value);
            else if (result is Fail<TFailure> fail) failure(fail.Value);    
        }

        public static Result Rail<TInput, TSuccess>(Result result, Func<TInput, Result> success)
        {
            return result switch
            {
                Pass<TInput> pass => Result.Pass(success(pass.Value)),
                Fail fail => fail,
                _ => throw new Exception()
            };
        }

        public static void Choose(Func<Result>[] funcs)
        {
            foreach (var func in funcs)
            {
                if (func() is Pass) continue;
                else break;
            }
        }
    }
}
