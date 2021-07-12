using FrostScript.Expressions;
using FrostScript.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FrostScript.DataTypes
{
    public interface IDataType { }

    public class AnyType : IDataType 
    {
        public override string ToString()
        {
            return $"any";
        }
    }
    public class IntType : IDataType 
    {
        public override string ToString()
        {
            return $"int";
        }
    }
    public class DoubleType : IDataType
    {
        public override string ToString()
        {
            return $"double";
        }
    }
    public class BoolType : IDataType
    {
        public override string ToString()
        {
            return $"bool";
        }
    }
    public class StringType : IDataType
    {
        public override string ToString()
        {
            return $"string";
        }
    }
    public class VoidType : IDataType
    {
        public override string ToString()
        {
            return $"void";
        }
    }
    public class FunctionType : IDataType
    {
        public IDataType ParameterType { get; }
        public IDataType Result { get; }

        public FunctionType(IDataType parameter, IDataType result)
        {
            ParameterType = parameter;
            Result = result;
        }

        public override bool Equals(object obj)
        {
            if (obj is FunctionType function)
            {
                return function.ParameterType == ParameterType && function.Result == Result;
            }
            else return false;
        }

        public static bool operator ==(FunctionType obj1, object obj2)
        {
            return obj1.Equals(obj2);
        }

        public static bool operator !=(FunctionType obj1, object obj2)
        {
            return !obj1.Equals(obj2);
        }

        public override string ToString()
        {
            return $"(fun {ParameterType} -> {Result})";
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
