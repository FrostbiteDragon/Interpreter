using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class ValidationException : Exception
    {
        public int Line { get; }
        public int CharacterPos { get; }
        public override string Message { get; }

        public ValidationException(int line, int characterPos, string message)
        {
            Line = line;
            CharacterPos = characterPos;
            Message = message;
        }

        public ValidationException(Token token, string message)
        {
            Line = token.Line;
            CharacterPos = token.Character;
            Message = message;
        }
    }
}
