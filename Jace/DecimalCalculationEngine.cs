using Jace.Execution;
using Jace.Operations;
using Jace.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace Jace
{
    public class DecimalCalculationEngine : CalculationEngineBase<decimal>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class with
        /// default parameters.
        /// </summary>
        public DecimalCalculationEngine()
            : this(CultureInfo.CurrentCulture, ExecutionMode.Compiled)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class. The dynamic compiler
        /// is used for formula execution and the optimizer and cache are enabled.
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> required for correctly reading floating poin numbers.
        /// </param>
        public DecimalCalculationEngine(CultureInfo cultureInfo)
            : this(cultureInfo, ExecutionMode.Compiled)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class. The optimizer and 
        /// cache are enabled.
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> required for correctly reading floating poin numbers.
        /// </param>
        /// <param name="executionMode">The execution mode that must be used for formula execution.</param>
        public DecimalCalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode)
            : this(cultureInfo, executionMode, true, true)
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CalculationEngine"/> class.
        /// </summary>
        /// <param name="cultureInfo">
        /// The <see cref="CultureInfo"/> required for correctly reading floating poin numbers.
        /// </param>
        /// <param name="executionMode">The execution mode that must be used for formula execution.</param>
        /// <param name="cacheEnabled">Enable or disable caching of mathematical formulas.</param>
        /// <param name="optimizerEnabled">Enable or disable optimizing of formulas.</param>
        public DecimalCalculationEngine(CultureInfo cultureInfo, ExecutionMode executionMode, bool cacheEnabled, bool optimizerEnabled)
            : base(cultureInfo, cacheEnabled, optimizerEnabled, 
            CreateExecutor(executionMode), new Optimizer<decimal>(new Interpreter<decimal>(DecimalNumericalOperations.Instance)), DecimalNumericalOperations.Instance)
        {
            // Register the default constants of Jace.NET into the constant registry
            RegisterDefaultConstants();

            // Register the default functions of Jace.NET into the function registry
            RegisterDefaultFunctions();
        }

        private static IExecutor<decimal> CreateExecutor(ExecutionMode executionMode)
        {
            if (executionMode == ExecutionMode.Interpreted)
            {
                return new Interpreter<decimal>(DecimalNumericalOperations.Instance);
            }
            else if (executionMode == ExecutionMode.Compiled)
            {
                return new ExpressionExecutor<decimal>((variables, functionRegistry) => new FormulaContext<decimal>(variables, functionRegistry), DecimalNumericalOperations.Instance);
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported execution mode \"{0}\".", executionMode),
                    "executionMode");
            }
        }

        private void RegisterDefaultFunctions()
        {
            FunctionRegistry.RegisterFunction("sin", (Func<decimal, decimal>)((a) => (decimal)Math.Sin((double)a)), false);
            FunctionRegistry.RegisterFunction("cos", (Func<decimal, decimal>)((a) => (decimal)Math.Cos((double)a)), false);
            FunctionRegistry.RegisterFunction("csc", (Func<decimal, decimal>)((a) => (decimal)MathUtil.Csc((double)a)), false);
            FunctionRegistry.RegisterFunction("sec", (Func<decimal, decimal>)((a) => (decimal)MathUtil.Sec((double)a)), false);
            FunctionRegistry.RegisterFunction("asin", (Func<decimal, decimal>)((a) => (decimal)Math.Asin((double)a)), false);
            FunctionRegistry.RegisterFunction("acos", (Func<decimal, decimal>)((a) => (decimal)Math.Acos((double)a)), false);
            FunctionRegistry.RegisterFunction("tan", (Func<decimal, decimal>)((a) => (decimal)Math.Tan((double)a)), false);
            FunctionRegistry.RegisterFunction("cot", (Func<decimal, decimal>)((a) => (decimal)MathUtil.Cot((double)a)), false);
            FunctionRegistry.RegisterFunction("atan", (Func<decimal, decimal>)((a) => (decimal)Math.Atan((double)a)), false);
            FunctionRegistry.RegisterFunction("acot", (Func<decimal, decimal>)((a) => (decimal)MathUtil.Acot((double)a)), false);
            FunctionRegistry.RegisterFunction("loge", (Func<decimal, decimal>)((a) => (decimal)Math.Log((double)a)), false);
            FunctionRegistry.RegisterFunction("log10", (Func<decimal, decimal>)((a) => (decimal)Math.Log10((double)a)), false);
            FunctionRegistry.RegisterFunction("logn", (Func<decimal, decimal, decimal>)((a, b) => (decimal)Math.Log((double)a, (double)b)), false);
            FunctionRegistry.RegisterFunction("sqrt", (Func<decimal, decimal>)((a) => (decimal)Math.Sqrt((double)a)), false);
            FunctionRegistry.RegisterFunction("abs", (Func<decimal, decimal>)((a) => Math.Abs(a)), false);
            FunctionRegistry.RegisterFunction("max", (Func<decimal, decimal, decimal>)((a, b) => Math.Max(a, b)), false);
            FunctionRegistry.RegisterFunction("min", (Func<decimal, decimal, decimal>)((a, b) => Math.Min(a, b)), false);
            FunctionRegistry.RegisterFunction("if", (Func<decimal, decimal, decimal, decimal>)((a, b, c) => (a != 0.0m ? b : c)), false);
            FunctionRegistry.RegisterFunction("ifless", (Func<decimal, decimal, decimal, decimal, decimal>)((a, b, c, d) => (a < b ? c : d)), false);
            FunctionRegistry.RegisterFunction("ifmore", (Func<decimal, decimal, decimal, decimal, decimal>)((a, b, c, d) => (a > b ? c : d)), false);
            FunctionRegistry.RegisterFunction("ifequal", (Func<decimal, decimal, decimal, decimal, decimal>)((a, b, c, d) => (a == b ? c : d)), false);
            FunctionRegistry.RegisterFunction("ceiling", (Func<decimal, decimal>)((a) => Math.Ceiling(a)), false);
            FunctionRegistry.RegisterFunction("floor", (Func<decimal, decimal>)((a) => Math.Floor(a)), false);
#if !WINDOWS_PHONE_7
            FunctionRegistry.RegisterFunction("truncate", (Func<decimal, decimal>)((a) => Math.Truncate(a)), false);
#endif
        }

        private void RegisterDefaultConstants()
        {
            ConstantRegistry.RegisterConstant("e", (decimal)Math.E, false);
            ConstantRegistry.RegisterConstant("pi", (decimal)Math.PI, false);
        }
    }

}
