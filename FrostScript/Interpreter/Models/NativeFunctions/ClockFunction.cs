using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.NativeFunctions
{
    public class ClockFunction : ICallable
    {
        public IDataType Type => DataType.Void;

        public object Call(object argument)
        {
            return Interpreter.Stopwatch.ElapsedMilliseconds;
        }
    }
}
