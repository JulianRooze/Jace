using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Jace.Operations;
using Jace.Util;

namespace Jace.Execution
{
    public class Interpreter<T> : IExecutor<T>
    {
        private readonly INumericalOperations<T> numericalOperations;

        public Interpreter(INumericalOperations<T> numericalOperations)
        {
            this.numericalOperations = numericalOperations;
        }

        public Func<IDictionary<string, T>, T> BuildFormula(Operation operation,
            IFunctionRegistry functionRegistry)
        {
            return variables =>
            {
                variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                return Execute(operation, functionRegistry, variables);
            };
        }

        public T Execute(Operation operation, IFunctionRegistry functionRegistry)
        {
            return Execute(operation, functionRegistry, new Dictionary<string, T>());
        }

        public T Execute(Operation operation, IFunctionRegistry functionRegistry,
            IDictionary<string, T> variables)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (operation.GetType() == typeof(IntegerConstant))
            {
                IntegerConstant constant = (IntegerConstant)operation;
                return this.numericalOperations.ConvertFromInt32(constant.Value);
            }
            else if (operation.GetType() == typeof(FloatingPointConstant<T>))
            {
                FloatingPointConstant<T> constant = (FloatingPointConstant<T>)operation;
                return constant.Value;
            }
            else if (operation.GetType() == typeof(Variable))
            {
                Variable variable = (Variable)operation;

                T value;
                bool variableFound = variables.TryGetValue(variable.Name, out value);

                if (variableFound)
                    return value;
                else
                    throw new VariableNotDefinedException(string.Format("The variable \"{0}\" used is not defined.", variable.Name));
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                Multiplication multiplication = (Multiplication)operation;
                return this.numericalOperations.Multiply(Execute(multiplication.Argument1, functionRegistry, variables), Execute(multiplication.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Addition))
            {
                Addition addition = (Addition)operation;
                return this.numericalOperations.Add(Execute(addition.Argument1, functionRegistry, variables), Execute(addition.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                Subtraction addition = (Subtraction)operation;
                return this.numericalOperations.Subtract(Execute(addition.Argument1, functionRegistry, variables), Execute(addition.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Division))
            {
                Division division = (Division)operation;
                return this.numericalOperations.Divide(Execute(division.Dividend, functionRegistry, variables), Execute(division.Divisor, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Modulo))
            {
                Modulo division = (Modulo)operation;
                return this.numericalOperations.Modulo(Execute(division.Dividend, functionRegistry, variables), Execute(division.Divisor, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                Exponentiation exponentiation = (Exponentiation)operation;
                return this.numericalOperations.Pow(Execute(exponentiation.Base, functionRegistry, variables), Execute(exponentiation.Exponent, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(UnaryMinus))
            {
                UnaryMinus unaryMinus = (UnaryMinus)operation;
                return this.numericalOperations.Negate(Execute(unaryMinus.Argument, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(LessThan))
            {
                LessThan lessThan = (LessThan)operation;
                return this.numericalOperations.LessThan(Execute(lessThan.Argument1, functionRegistry, variables), Execute(lessThan.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(LessOrEqualThan))
            {
                LessOrEqualThan lessOrEqualThan = (LessOrEqualThan)operation;
                return this.numericalOperations.LessOrEqualThan(Execute(lessOrEqualThan.Argument1, functionRegistry, variables), Execute(lessOrEqualThan.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(GreaterThan))
            {
                GreaterThan greaterThan = (GreaterThan)operation;
                return this.numericalOperations.GreaterThan(Execute(greaterThan.Argument1, functionRegistry, variables), Execute(greaterThan.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(GreaterOrEqualThan))
            {
                GreaterOrEqualThan greaterOrEqualThan = (GreaterOrEqualThan)operation;
                return this.numericalOperations.GreaterOrEqualThan(Execute(greaterOrEqualThan.Argument1, functionRegistry, variables), Execute(greaterOrEqualThan.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Equal))
            {
                Equal equal = (Equal)operation;
                return this.numericalOperations.Equal(Execute(equal.Argument1, functionRegistry, variables), Execute(equal.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(NotEqual))
            {
                NotEqual notEqual = (NotEqual)operation;
                return this.numericalOperations.NotEqual(Execute(notEqual.Argument1, functionRegistry, variables), Execute(notEqual.Argument2, functionRegistry, variables));
            }
            else if (operation.GetType() == typeof(Function))
            {
                Function function = (Function)operation;

                FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);

                T[] arguments = new T[functionInfo.NumberOfParameters];
                for (int i = 0; i < arguments.Length; i++)
                    arguments[i] = Execute(function.Arguments[i], functionRegistry, variables);

                return Invoke(functionInfo.Function, arguments);
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported operation \"{0}\".", operation.GetType().FullName), "operation");
            }
        }

        private T Invoke(Delegate function, T[] arguments)
        {
            // DynamicInvoke is slow, so we first try to convert it to a Func
            if (function is Func<T>)
            {
                return ((Func<T>)function).Invoke();
            }
            else if (function is Func<T, T>)
            {
                return ((Func<T, T>)function).Invoke(arguments[0]);
            }
            else if (function is Func<T, T, T>)
            {
                return ((Func<T, T, T>)function).Invoke(arguments[0], arguments[1]);
            }
            else if (function is Func<T, T, T, T>)
            {
                return ((Func<T, T, T, T>)function).Invoke(arguments[0], arguments[1], arguments[2]);
            }
            else if (function is Func<T, T, T, T, T>)
            {
                return ((Func<T, T, T, T, T>)function).Invoke(arguments[0], arguments[1], arguments[2], arguments[3]);
            }
#if !WINDOWS_PHONE_7
            else if (function is Func<T, T, T, T, T, T>)
            {
                return ((Func<T, T, T, T, T, T>)function).Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4]);
            }
            else if (function is Func<T, T, T, T, T, T, T>)
            {
                return ((Func<T, T, T, T, T, T, T>)function).Invoke(arguments[0], arguments[1], arguments[2], arguments[3], arguments[4], arguments[5]);
            }
#endif
            else
            {
                return (T)function.DynamicInvoke((from s in arguments select (object)s).ToArray());
            }
        }
    }

    public class Interpreter : Interpreter<double>
    {
        public Interpreter() : base(DoubleNumericalOperations.Instance) { }
    }
}
