using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Jace.Util
{
    /// <summary>
    /// Utility methods of Jace.NET that can be used throughout the engine.
    /// </summary>
    public  static class EngineUtil
    {
        static internal IDictionary<string, T> ConvertVariableNamesToLowerCase<T>(IDictionary<string, T> variables)
        {
            var temp = new Dictionary<string, T>();
            foreach (var keyValuePair in variables)
            {
                temp.Add(keyValuePair.Key.ToLowerInvariant(), keyValuePair.Value);
            }

            return temp;
        }
    }
}
