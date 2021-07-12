using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class TypeException : Exception
    {
        public int Line { get; }
        public int CharacterPos { get; }
        public override string Message { get; }

        public TypeException(int line, int characterPos, string message)
        {
            Line = line;
            CharacterPos = characterPos;
            Message = message;
        }

        public TypeException(Token token, string message)
        {
            Line = token.Line;
            CharacterPos = token.Character;
            Message = message;
        }
    }
}
