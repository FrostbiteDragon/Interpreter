using Frostware.Result;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public static class Railroad
    {
        public static Func<T, Result> Fish<T, T2>(this Func<T, Result> previous, Func<T2, Result> next)
        {
            return (arg) =>
            {
                var result = previous(arg);

                return result switch
                {
                    Pass<T2> pass => next(pass.Value),
                    Pass pass => throw new ArgumentException($"expected {typeof(Pass<T2>)}, given {pass}"),
                    Fail => result,
                };
            };
        }

    }
}
