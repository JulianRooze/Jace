using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jace.Execution
{
    public interface INumericalOperations<T>
    {
        T Multiply(T n1, T n2);
        T Add(T n1, T n2);
        T Subtract(T n1, T n2);
        T Modulo(T n1, T n2);
        T Divide(T n1, T n2);
        T Pow(T n, T exponent);
        T Negate(T n);
        T LessThan(T n1, T n2);
        T LessOrEqualThan(T n1, T n2);
        T GreaterThan(T n1, T n2);
        T GreaterOrEqualThan(T n1, T n2);
        T Equal(T n1, T n2);
        T NotEqual(T n1, T n2);
        bool TryParseFloatingPoint(string str, CultureInfo cultureInfo, out T numericalValue);
        INumericConstants<T> Constants { get; }
        T ConvertFromInt32(int i);
    }

    public interface INumericConstants<T>
    {
        T Zero { get;}
        T One { get; }
    }

    public class DecimalNumericalOperations : INumericalOperations<decimal>
    {
        public static readonly DecimalNumericalOperations Instance = new DecimalNumericalOperations();
        private DecimalNumericalOperations.DecimalConstants _constants;
        private DecimalNumericalOperations() 
        {
            _constants = new DecimalConstants();
        }

        public decimal Multiply(decimal n1, decimal n2)
        {
            return n1 * n2;
        }

        public decimal Add(decimal n1, decimal n2)
        {
            return n1 + n2;
        }

        public decimal Subtract(decimal n1, decimal n2)
        {
            return n1 - n2;
        }

        public decimal Modulo(decimal n1, decimal n2)
        {
            return n1 % n2;
        }

        public decimal Divide(decimal n1, decimal n2)
        {
            return n1 / n2;
        }

        public decimal Pow(decimal n, decimal exponent)
        {
            return (decimal)Math.Pow((double)n, (double)exponent);
        }

        public decimal Negate(decimal n)
        {
            return -n;
        }

        public decimal LessThan(decimal n1, decimal n2)
        {
            return n1 < n2 ? 1.0m : 0.0m;
        }

        public decimal LessOrEqualThan(decimal n1, decimal n2)
        {
            return n1 <= n2 ? 1.0m : 0.0m;
        }

        public decimal GreaterThan(decimal n1, decimal n2)
        {
            return n1 > n2 ? 1.0m : 0.0m;
        }

        public decimal GreaterOrEqualThan(decimal n1, decimal n2)
        {
            return n1 >= n2 ? 1.0m : 0.0m;
        }

        public decimal Equal(decimal n1, decimal n2)
        {
            return n1 == n2 ? 1.0m : 0.0m;
        }

        public decimal NotEqual(decimal n1, decimal n2)
        {
            return n1 != n2 ? 1.0m : 0.0m;
        }


        public bool TryParseFloatingPoint(string str, CultureInfo cultureInfo, out decimal numericalValue)
        {
            return decimal.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, cultureInfo, out numericalValue);
        }

        private class DecimalConstants : INumericConstants<decimal>
        {
            public decimal Zero
            {
                get { return 0; }
            }

            public decimal One
            {
                get { return 1; }
            }
        }

        public INumericConstants<decimal> Constants
        {
            get { return _constants; }
        }


        public decimal ConvertFromInt32(int i)
        {
            return i;
        }
    }

    public class DoubleNumericalOperations : INumericalOperations<double>
    {
        
        public static readonly DoubleNumericalOperations Instance = new DoubleNumericalOperations();
        private DoubleNumericalOperations.DoubleConstants _constants;
        private DoubleNumericalOperations() 
        {
            _constants = new DoubleConstants();
        }
        public double Multiply(double n1, double n2)
        {
            return n1 * n2;
        }

        public double Add(double n1, double n2)
        {
            return n1 + n2;
        }

        public double Subtract(double n1, double n2)
        {
            return n1 - n2;
        }

        public double Modulo(double n1, double n2)
        {
            return n1 % n2;
        }

        public double Divide(double n1, double n2)
        {
            return n1 / n2;
        }

        public double Pow(double n, double exponent)
        {
            return (double)Math.Pow((double)n, (double)exponent);
        }

        public double Negate(double n)
        {
            return -n;
        }

        public double LessThan(double n1, double n2)
        {
            return n1 < n2 ? 1.0 : 0.0;
        }

        public double LessOrEqualThan(double n1, double n2)
        {
            return n1 <= n2 ? 1.0 : 0.0;
        }

        public double GreaterThan(double n1, double n2)
        {
            return n1 > n2 ? 1.0 : 0.0;
        }

        public double GreaterOrEqualThan(double n1, double n2)
        {
            return n1 >= n2 ? 1.0 : 0.0;
        }

        public double Equal(double n1, double n2)
        {
            return n1 == n2 ? 1.0 : 0.0;
        }

        public double NotEqual(double n1, double n2)
        {
            return n1 != n2 ? 1.0 : 0.0;
        }

        public bool TryParseFloatingPoint(string str, CultureInfo cultureInfo, out double numericalValue)
        {
            return double.TryParse(str, NumberStyles.Float | NumberStyles.AllowThousands, cultureInfo, out numericalValue);
        }

        private class DoubleConstants : INumericConstants<double>
        {
            public double Zero
            {
                get { return 0; }
            }

            public double One
            {
                get { return 1; }
            }
        }

        public INumericConstants<double> Constants
        {
            get { return _constants; }
        }


        public double ConvertFromInt32(int i)
        {
            return i;
        }
    }
}
