using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript
{
    public class ParseException : Exception
    {
        public int Line { get; }
        public int CharacterPos { get; }
        public override string Message { get; }

        public int PickupPoint { get; }

        public ParseException(int line, int character, string message, int pickupPoint)
        {
            Line = line;
            CharacterPos = character;
            Message = message;
            PickupPoint = pickupPoint;
        }

        public ParseException(Token token, string message, int pickupPoint)
        {
            Line = token.Line;
            CharacterPos = token.Character;
            Message = message;
            PickupPoint = pickupPoint;
        }
    }
}
