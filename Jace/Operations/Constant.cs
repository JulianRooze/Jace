using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jace.Operations
{
    public abstract class Constant<T> : Operation
    {
        public Constant(DataType dataType, T value)
            : base(dataType, false)
        {
            this.Value = value;
        }

        public T Value { get; private set; }

        public override bool Equals(object obj)
        {
            Constant<T> other = obj as Constant<T>;
            if (other != null)
                return this.Value.Equals(other.Value);
            else
                return false;
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }
    }

    public class IntegerConstant : Constant<int>
    {
        public IntegerConstant(int value)
            : base(DataType.Integer, value)
        {
        }
    }

    public class FloatingPointConstant<T> : Constant<T>
    {
        public FloatingPointConstant(T value)
            : base(DataType.FloatingPoint, value)
        {
        }
    }

    public interface IFloatingPointConstantProvider
    {
      bool TryParse(string str, CultureInfo cultureInfo, out object value);
    }

    public class FloatingPointConstantProvider : IFloatingPointConstantProvider
    {

        public bool TryParse(string str, CultureInfo cultureInfo, out object value)
        {
          double val;
          var success = double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands,
                              cultureInfo, out val);

          value = val;

          return success;
        }
    }

    public class DecimalFloatingPointConstantProvider : IFloatingPointConstantProvider
    {
        public bool TryParse(string str, CultureInfo cultureInfo, out object value)
        {
          decimal val;
          var success = decimal.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands,
                              cultureInfo, out val);

          value = val;

          return success;
        }
    }
}
