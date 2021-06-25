using FrostScript.Expressions;
using FrostScript.Statements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.DataTypes
{
    public interface IDataType { }

    public class AnyType : IDataType { }
    public class IntType : IDataType { }
    public class DoubleType : IDataType { }
    public class BoolType : IDataType { }
    public class StringType : IDataType { }
    public class VoidType : IDataType { }
    public class FunctionType : IDataType
    {
        public IDataType Parameter { get; }
        public IDataType Result { get; }

        public FunctionType(IDataType parameter, IDataType result)
        {
            Parameter = parameter;
            Result = result;
        }
    }

    public class DataType
    {
        static readonly AnyType _any = new AnyType();
        public static AnyType Any => _any;

        static readonly IntType _int = new IntType();
        public static IntType Int => _int;

        static readonly DoubleType _double = new DoubleType();
        public static DoubleType Double => _double;

        static readonly BoolType _bool = new BoolType();
        public static BoolType Bool => _bool;

        static readonly StringType _string = new StringType();
        public static StringType String => _string;

        static readonly VoidType _void = new VoidType();
        public static VoidType Void => _void;

        public static FunctionType Function(IDataType parameter, IDataType result) => new FunctionType(parameter, result);
    }

}
