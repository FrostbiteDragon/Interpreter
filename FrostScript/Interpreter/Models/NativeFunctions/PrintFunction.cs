using FrostScript.DataTypes;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;

namespace FrostScript.NativeFunctions
{
    public class PrintFunction : ICallable
    {
        public IDataType Type => DataType.Function(DataType.Any, DataType.Void);

        public object Call(object argument)
        {

            if (argument is ICollection<dynamic> collection)
            {
                Console.WriteLine("List: ");
                foreach (var item in collection)
                    Console.WriteLine($"    {item}");
            }
            else
                Console.WriteLine(argument);

            return null;
        }

        public override string ToString()
        {
            return "any -> void";
        }
    }
}
