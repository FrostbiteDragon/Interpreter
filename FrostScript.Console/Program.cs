using System.IO;
using Frostware.Pipe;
using static FrostScript.FrostScript;

args[0]
.Pipe(File.ReadAllText)
.Pipe(ExecuteString);