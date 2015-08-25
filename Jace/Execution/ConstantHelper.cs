using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jace.Execution
{
    public static class ConstantHelper
    {
        public static object Convert<T>(int i)
        {
            if (typeof(T) == typeof(decimal))
            {
                return System.Convert.ToDecimal(i);
            }
            else if (typeof(T) == typeof(double))
            {
                return System.Convert.ToDouble(i);
            }
            else
            {
                return i;
            }
        }
    }
}
