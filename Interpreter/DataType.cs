namespace FrostScript
{
    public enum DataType { Integer, Decimal }

    public class Data
    {
        public DataType Type { get; init; }
        public object Value { get; init; }
    }
}