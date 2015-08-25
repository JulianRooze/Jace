﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Jace.Operations;
using Jace.Util;

namespace Jace.Execution
{

    public class ExpressionExecutor<T> : IExecutor<T>
    {
        private readonly Func<IDictionary<string, T>, IFunctionRegistry, IFormulaContext<T>> formulaContextFactory;

        public ExpressionExecutor(Func<IDictionary<string, T>, IFunctionRegistry, IFormulaContext<T>> formulaContextFactory)
        {
            if (formulaContextFactory == null) throw new ArgumentNullException("formulaContextFactory");

            this.formulaContextFactory = formulaContextFactory;
        }

        public T Execute(Operation operation, IFunctionRegistry functionRegistry)
        {
            return Execute(operation, functionRegistry, new Dictionary<string, T>());
        }

        public T Execute(Operation operation, IFunctionRegistry functionRegistry,
            IDictionary<string, T> variables)
        {
            return BuildFormula(operation, functionRegistry)(variables);
        }

        public Func<IDictionary<string, T>, T> BuildFormula(Operation operation,
            IFunctionRegistry functionRegistry)
        {
            var func = BuildFormulaInternal(operation, functionRegistry);
            return variables =>
            {
                variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                var context = this.formulaContextFactory(variables, functionRegistry);
                return func(context);
            };
        }

        private Func<IFormulaContext<T>, T> BuildFormulaInternal(Operation operation,
            IFunctionRegistry functionRegistry)
        {
            ParameterExpression contextParameter = Expression.Parameter(typeof(IFormulaContext<T>), "context");

            LabelTarget returnLabel = Expression.Label(typeof(T));

            return Expression.Lambda<Func<IFormulaContext<T>, T>>(
                Expression.Block(
                    Expression.Return(returnLabel, GenerateMethodBody(operation, contextParameter, functionRegistry)),
                    Expression.Label(returnLabel, Expression.Constant(ConstantHelper.Convert<T>(0)))
                ),
                contextParameter
            ).Compile();
        }

        private Expression GenerateMethodBody(Operation operation, ParameterExpression contextParameter,
            IFunctionRegistry functionRegistry)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (operation.GetType() == typeof(IntegerConstant))
            {
                IntegerConstant constant = (IntegerConstant)operation;

                return Expression.Convert(Expression.Constant(constant.Value, typeof(int)), typeof(T));
            }
            else if (operation.GetType() == typeof(FloatingPointConstant<T>))
            {
                var constant = (FloatingPointConstant<T>)operation;

                return Expression.Constant(constant.Value, typeof(T));
            }
            else if (operation.GetType() == typeof(Variable))
            {
                Type contextType = typeof(IFormulaContext<T>);
                Type dictionaryType = typeof(IDictionary<string, T>);

                Variable variable = (Variable)operation;

                Expression getVariables = Expression.Property(contextParameter, "Variables");
                ParameterExpression value = Expression.Variable(typeof(T), "value");

                Expression variableFound = Expression.Call(getVariables,
                    dictionaryType.GetMethod("TryGetValue", new Type[] { typeof(string), typeof(T).MakeByRefType() }),
                    Expression.Constant(variable.Name),
                    value);

                Expression throwException = Expression.Throw(
                    Expression.New(typeof(VariableNotDefinedException).GetConstructor(new Type[] { typeof(string) }),
                        Expression.Constant(string.Format("The variable \"{0}\" used is not defined.", variable.Name))));

                LabelTarget returnLabel = Expression.Label(typeof(T));

                return Expression.Block(
                    new[] { value },
                    Expression.IfThenElse(
                        variableFound,
                        Expression.Return(returnLabel, value),
                        throwException
                    ),
                    Expression.Label(returnLabel, Expression.Constant(ConstantHelper.Convert<T>(0)))
                );
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                Multiplication multiplication = (Multiplication)operation;
                Expression argument1 = GenerateMethodBody(multiplication.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(multiplication.Argument2, contextParameter, functionRegistry);

                return Expression.Multiply(argument1, argument2);
            }
            else if (operation.GetType() == typeof(Addition))
            {
                Addition addition = (Addition)operation;
                Expression argument1 = GenerateMethodBody(addition.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(addition.Argument2, contextParameter, functionRegistry);

                return Expression.Add(argument1, argument2);
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                Subtraction addition = (Subtraction)operation;
                Expression argument1 = GenerateMethodBody(addition.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(addition.Argument2, contextParameter, functionRegistry);

                return Expression.Subtract(argument1, argument2);
            }
            else if (operation.GetType() == typeof(Division))
            {
                Division division = (Division)operation;
                Expression dividend = GenerateMethodBody(division.Dividend, contextParameter, functionRegistry);
                Expression divisor = GenerateMethodBody(division.Divisor, contextParameter, functionRegistry);

                return Expression.Divide(dividend, divisor);
            }
            else if (operation.GetType() == typeof(Modulo))
            {
                Modulo modulo = (Modulo)operation;
                Expression dividend = GenerateMethodBody(modulo.Dividend, contextParameter, functionRegistry);
                Expression divisor = GenerateMethodBody(modulo.Divisor, contextParameter, functionRegistry);

                return Expression.Modulo(dividend, divisor);
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                Exponentiation exponentation = (Exponentiation)operation;
                Expression @base = GenerateMethodBody(exponentation.Base, contextParameter, functionRegistry);
                Expression exponent = GenerateMethodBody(exponentation.Exponent, contextParameter, functionRegistry);

                return Expression.Call(null, typeof(Math).GetMethod("Pow", new Type[] { typeof(T), typeof(T) }), @base, exponent);
            }
            else if (operation.GetType() == typeof(UnaryMinus))
            {
                UnaryMinus unaryMinus = (UnaryMinus)operation;
                Expression argument = GenerateMethodBody(unaryMinus.Argument, contextParameter, functionRegistry);
                return Expression.Negate(argument);
            }
            else if (operation.GetType() == typeof(LessThan))
            {
                LessThan lessThan = (LessThan)operation;
                Expression argument1 = GenerateMethodBody(lessThan.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(lessThan.Argument2, contextParameter, functionRegistry);

                return Expression.Condition(Expression.LessThan(argument1, argument2),
                    Expression.Constant(ConstantHelper.Convert<T>(1)),
                    Expression.Constant(ConstantHelper.Convert<T>(0)));
            }
            else if (operation.GetType() == typeof(LessOrEqualThan))
            {
                LessOrEqualThan lessOrEqualThan = (LessOrEqualThan)operation;
                Expression argument1 = GenerateMethodBody(lessOrEqualThan.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(lessOrEqualThan.Argument2, contextParameter, functionRegistry);

                return Expression.Condition(Expression.LessThanOrEqual(argument1, argument2),
                    Expression.Constant(ConstantHelper.Convert<T>(1)),
                    Expression.Constant(ConstantHelper.Convert<T>(0)));
            }
            else if (operation.GetType() == typeof(GreaterThan))
            {
                GreaterThan greaterThan = (GreaterThan)operation;
                Expression argument1 = GenerateMethodBody(greaterThan.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(greaterThan.Argument2, contextParameter, functionRegistry);

                return Expression.Condition(Expression.GreaterThan(argument1, argument2),
                    Expression.Constant(ConstantHelper.Convert<T>(1)),
                    Expression.Constant(ConstantHelper.Convert<T>(0)));
            }
            else if (operation.GetType() == typeof(GreaterOrEqualThan))
            {
                GreaterOrEqualThan greaterOrEqualThan = (GreaterOrEqualThan)operation;
                Expression argument1 = GenerateMethodBody(greaterOrEqualThan.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(greaterOrEqualThan.Argument2, contextParameter, functionRegistry);

                return Expression.Condition(Expression.GreaterThanOrEqual(argument1, argument2),
                    Expression.Constant(ConstantHelper.Convert<T>(1)),
                    Expression.Constant(ConstantHelper.Convert<T>(0)));
            }
            else if (operation.GetType() == typeof(Equal))
            {
                Equal equal = (Equal)operation;
                Expression argument1 = GenerateMethodBody(equal.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(equal.Argument2, contextParameter, functionRegistry);

                return Expression.Condition(Expression.Equal(argument1, argument2),
                    Expression.Constant(ConstantHelper.Convert<T>(1)),
                    Expression.Constant(ConstantHelper.Convert<T>(0)));
            }
            else if (operation.GetType() == typeof(NotEqual))
            {
                NotEqual notEqual = (NotEqual)operation;
                Expression argument1 = GenerateMethodBody(notEqual.Argument1, contextParameter, functionRegistry);
                Expression argument2 = GenerateMethodBody(notEqual.Argument2, contextParameter, functionRegistry);

                return Expression.Condition(Expression.NotEqual(argument1, argument2),
                    Expression.Constant(ConstantHelper.Convert<T>(1)),
                    Expression.Constant(ConstantHelper.Convert<T>(0)));
            }
            else if (operation.GetType() == typeof(Function))
            {
                Function function = (Function)operation;

                FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);
                Type funcType = GetFuncType(functionInfo.NumberOfParameters);
                Type[] parameterTypes = (from i in Enumerable.Range(0, functionInfo.NumberOfParameters)
                                         select typeof(T)).ToArray();

                Expression[] arguments = new Expression[functionInfo.NumberOfParameters];
                for (int i = 0; i < functionInfo.NumberOfParameters; i++)
                    arguments[i] = GenerateMethodBody(function.Arguments[i], contextParameter, functionRegistry);

                Expression getFunctionRegistry = Expression.Property(contextParameter, "FunctionRegistry");

                ParameterExpression functionInfoVariable = Expression.Variable(typeof(FunctionInfo));

                return Expression.Block(
                    new[] { functionInfoVariable },
                    Expression.Assign(
                        functionInfoVariable,
                        Expression.Call(getFunctionRegistry, typeof(IFunctionRegistry).GetMethod("GetFunctionInfo", new Type[] { typeof(string) }), Expression.Constant(function.FunctionName))
                    ),
                    Expression.Call(
                        Expression.Convert(Expression.Property(functionInfoVariable, "Function"), funcType),
                        funcType.GetMethod("Invoke", parameterTypes),
                        arguments));
            }
            else
            {
                throw new ArgumentException(string.Format("Unsupported operation \"{0}\".", operation.GetType().FullName), "operation");
            }
        }

        private Type GetFuncType(int numberOfParameters)
        {
            string funcTypeName = string.Format("System.Func`{0}", numberOfParameters + 1);
            Type funcType = Type.GetType(funcTypeName);

            Type[] typeArguments = new Type[numberOfParameters + 1];
            for (int i = 0; i < typeArguments.Length; i++)
                typeArguments[i] = typeof(T);

            return funcType.MakeGenericType(typeArguments);
        }
    }
}
