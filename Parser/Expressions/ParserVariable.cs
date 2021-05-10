using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.Expressions
{
    public class ParserVariable
    {
        public string Id { get; init; }
        public DataType Type { get; init; }
        public bool Mutable { get; set; }
    }
}
