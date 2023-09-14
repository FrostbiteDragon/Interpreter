using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public static class Reporter
    {
        public static void Report(int line, int charactorPos, string message)
        {
            if (line == 0 && charactorPos == 0)
                Console.WriteLine(message);
            else
                Console.WriteLine($"[Line: {line}, Character: {charactorPos}] Error: {message}");
        }
    }
}
