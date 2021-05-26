using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.NativeFunctions
{
    public class PrintFunction : ICallable
    {
        public DataType Type => DataType.Void;

        public object Call(object argument)
        {
            Console.WriteLine(argument);
            return null;
        }
    }
}
