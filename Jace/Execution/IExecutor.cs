using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Operations;

namespace Jace.Execution
{
    public interface IExecutor<T>
    {
        T Execute(Operation operation, IFunctionRegistry functionRegistry);
        T Execute(Operation operation, IFunctionRegistry functionRegistry, IDictionary<string, T> variables);

        Func<IDictionary<string, T>, T> BuildFormula(Operation operation, IFunctionRegistry functionRegistry);
    }
}
