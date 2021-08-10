using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class InterpretException : Exception
    {
        public int Line { get; }
        public int CharacterPos { get; }
        public override string Message { get; }

        public InterpretException(int line, int character, string message)
        {
            Line = line;
            CharacterPos = character;
            Message = message;
        }

        public InterpretException(string message)
        {
            Message = message;
        }
    }
}
