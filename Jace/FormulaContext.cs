using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Execution;

namespace Jace
{
    public interface IFormulaContext<T>
    {
        IDictionary<string, T> Variables { get; }

        IFunctionRegistry FunctionRegistry { get; }
    }

    public class FormulaContext : IFormulaContext<double>
    {
        public FormulaContext(IDictionary<string, double> variables,
            IFunctionRegistry functionRegistry)
        {
            this.Variables = variables;
            this.FunctionRegistry = functionRegistry;
        }

        public IDictionary<string, double> Variables { get; private set; }

        public IFunctionRegistry FunctionRegistry { get; private set; }
    }

    public class DecimalFormulaContext : IFormulaContext<decimal>
    {
        public DecimalFormulaContext(IDictionary<string, decimal> variables,
            IFunctionRegistry functionRegistry)
        {
            this.Variables = variables;
            this.FunctionRegistry = functionRegistry;
        }

        public IDictionary<string, decimal> Variables { get; private set; }

        public IFunctionRegistry FunctionRegistry { get; private set; }
    }
}
