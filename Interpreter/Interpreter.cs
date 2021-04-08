using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Interpreter
{
    public static class Interpreter
    {
        public static void Execute(IEnumerable<Node> nodes)
        {
            foreach(var node in nodes)
            {
                switch (node.Oporator.value)
                {
                    case "+":
                        Console.WriteLine(int.Parse(node.Left.value) + int.Parse(node.Right.value));
                        break;
                }
            }
        }
    }
}
