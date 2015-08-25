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
    public class DecimalCalculationEngine : CalculationEngine<decimal>
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
        {
            this.executionFormulaCache = new MemoryCache<string, Func<IDictionary<string, decimal>, decimal>>();
            this.FunctionRegistry = new FunctionRegistry(false);
            this.ConstantRegistry = new ConstantRegistry<decimal>(false);
            this.cultureInfo = cultureInfo;
            this.cacheEnabled = cacheEnabled;
            this.optimizerEnabled = optimizerEnabled;
            this.floatingPointConstantProvider = new DecimalFloatingPointConstantProvider();

            if (executionMode == ExecutionMode.Interpreted)
            {
                executor = new DecimalInterpreter();
            }
            else if (executionMode == ExecutionMode.Compiled)
            {
                //executor = new DecimalDynamicCompiler();
                executor = new ExpressionExecutor<decimal>((variables, functionRegistry) => new DecimalFormulaContext(variables, functionRegistry));
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported execution mode \"{0}\".", executionMode),
                    "executionMode");
            }

            optimizer = new Optimizer<decimal>(new DecimalInterpreter()); // We run the optimizer with the interpreter 

            // Register the default constants of Jace.NET into the constant registry
            RegisterDefaultConstants();

            // Register the default functions of Jace.NET into the function registry
            RegisterDefaultFunctions();
        }


        private void RegisterDefaultFunctions()
        {
            FunctionRegistry.RegisterFunction("sin", (Func<double, double>)((a) => Math.Sin(a)), false);
            FunctionRegistry.RegisterFunction("cos", (Func<double, double>)((a) => Math.Cos(a)), false);
            FunctionRegistry.RegisterFunction("csc", (Func<double, double>)((a) => MathUtil.Csc(a)), false);
            FunctionRegistry.RegisterFunction("sec", (Func<double, double>)((a) => MathUtil.Sec(a)), false);
            FunctionRegistry.RegisterFunction("asin", (Func<double, double>)((a) => Math.Asin(a)), false);
            FunctionRegistry.RegisterFunction("acos", (Func<double, double>)((a) => Math.Acos(a)), false);
            FunctionRegistry.RegisterFunction("tan", (Func<double, double>)((a) => Math.Tan(a)), false);
            FunctionRegistry.RegisterFunction("cot", (Func<double, double>)((a) => MathUtil.Cot(a)), false);
            FunctionRegistry.RegisterFunction("atan", (Func<double, double>)((a) => Math.Atan(a)), false);
            FunctionRegistry.RegisterFunction("acot", (Func<double, double>)((a) => MathUtil.Acot(a)), false);
            FunctionRegistry.RegisterFunction("loge", (Func<double, double>)((a) => Math.Log(a)), false);
            FunctionRegistry.RegisterFunction("log10", (Func<double, double>)((a) => Math.Log10(a)), false);
            FunctionRegistry.RegisterFunction("logn", (Func<double, double, double>)((a, b) => Math.Log(a, b)), false);
            FunctionRegistry.RegisterFunction("sqrt", (Func<double, double>)((a) => Math.Sqrt(a)), false);
            FunctionRegistry.RegisterFunction("abs", (Func<double, double>)((a) => Math.Abs(a)), false);
            FunctionRegistry.RegisterFunction("max", (Func<double, double, double>)((a, b) => Math.Max(a, b)), false);
            FunctionRegistry.RegisterFunction("min", (Func<double, double, double>)((a, b) => Math.Min(a, b)), false);
            FunctionRegistry.RegisterFunction("if", (Func<double, double, double, double>)((a, b, c) => (a != 0.0 ? b : c)), false);
            FunctionRegistry.RegisterFunction("ifless", (Func<double, double, double, double, double>)((a, b, c, d) => (a < b ? c : d)), false);
            FunctionRegistry.RegisterFunction("ifmore", (Func<double, double, double, double, double>)((a, b, c, d) => (a > b ? c : d)), false);
            FunctionRegistry.RegisterFunction("ifequal", (Func<double, double, double, double, double>)((a, b, c, d) => (a == b ? c : d)), false);
            FunctionRegistry.RegisterFunction("ceiling", (Func<double, double>)((a) => Math.Ceiling(a)), false);
            FunctionRegistry.RegisterFunction("floor", (Func<double, double>)((a) => Math.Floor(a)), false);
#if !WINDOWS_PHONE_7
            FunctionRegistry.RegisterFunction("truncate", (Func<double, double>)((a) => Math.Truncate(a)), false);
#endif
        }

        private void RegisterDefaultConstants()
        {
            ConstantRegistry.RegisterConstant("e", (decimal)Math.E, false);
            ConstantRegistry.RegisterConstant("pi", (decimal)Math.PI, false);
        }
    }

}
