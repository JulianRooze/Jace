using System;
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
#if !NETFX_CORE
    public class DynamicCompiler : IExecutor<double>
    {
        public double Execute(Operation operation, IFunctionRegistry functionRegistry)
        {
            return Execute(operation, functionRegistry, new Dictionary<string, double>());
        }

        public double Execute(Operation operation, IFunctionRegistry functionRegistry, 
            IDictionary<string, double> variables)
        {
            return BuildFormula(operation, functionRegistry)(variables);
        }

        public Func<IDictionary<string, double>, double> BuildFormula(Operation operation,
            IFunctionRegistry functionRegistry)
        {
            Func<FormulaContext<double>, double> func = BuildFormulaInternal(operation, functionRegistry);
            return variables =>
                {
                    variables = EngineUtil.ConvertVariableNamesToLowerCase(variables);
                    var context = new FormulaContext<double>(variables, functionRegistry);
                    return func(context);
                };
        }

        private Func<FormulaContext<double>, double> BuildFormulaInternal(Operation operation,
            IFunctionRegistry functionRegistry)
        {
            DynamicMethod method = new DynamicMethod("MyCalcMethod", typeof(double),
                new Type[] { typeof(FormulaContext<double>) });
            GenerateMethodBody(method, operation, functionRegistry);

            Func<FormulaContext<double>, double> function =
                (Func<FormulaContext<double>, double>)method.CreateDelegate(typeof(Func<FormulaContext<double>, double>));

            return function;
        }

        private void GenerateMethodBody(DynamicMethod method, Operation operation, 
            IFunctionRegistry functionRegistry)
        {
            ILGenerator generator = method.GetILGenerator();
            generator.DeclareLocal(typeof(double));
            generator.DeclareLocal(typeof(object[]));
            GenerateMethodBody(generator, operation, functionRegistry);
            generator.Emit(OpCodes.Ret);
        }

        private void GenerateMethodBody(ILGenerator generator, Operation operation, 
            IFunctionRegistry functionRegistry)
        {
            if (operation == null)
                throw new ArgumentNullException("operation");

            if (operation.GetType() == typeof(IntegerConstant))
            {
                IntegerConstant constant = (IntegerConstant)operation;
                
                generator.Emit(OpCodes.Ldc_I4, constant.Value);
                generator.Emit(OpCodes.Conv_R8);
            }
            else if (operation.GetType() == typeof(FloatingPointConstant<double>))
            {
                FloatingPointConstant<double> constant = (FloatingPointConstant<double>)operation;

                generator.Emit(OpCodes.Ldc_R8, constant.Value);
            }
            else if (operation.GetType() == typeof(Variable))
            {
                Type dictionaryType = typeof(IDictionary<string, double>);

                Variable variable = (Variable)operation;

                Label throwExceptionLabel = generator.DefineLabel();
                Label returnLabel = generator.DefineLabel();

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, typeof(FormulaContext<double>).GetProperty("Variables").GetGetMethod());
                generator.Emit(OpCodes.Ldstr, variable.Name);
                generator.Emit(OpCodes.Ldloca_S, (byte)0);
                generator.Emit(OpCodes.Callvirt, dictionaryType.GetMethod("TryGetValue", new Type[] { typeof(string), typeof(double).MakeByRefType() }));
                generator.Emit(OpCodes.Ldc_I4_0);
                generator.Emit(OpCodes.Ceq);
                generator.Emit(OpCodes.Brtrue_S, throwExceptionLabel);

                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Br_S, returnLabel);

                generator.MarkLabel(throwExceptionLabel);
                generator.Emit(OpCodes.Ldstr, string.Format("The variable \"{0}\" used is not defined.", variable.Name));
                generator.Emit(OpCodes.Newobj, typeof(VariableNotDefinedException).GetConstructor(new Type[] { typeof(string) }));
                generator.Emit(OpCodes.Throw);

                generator.MarkLabel(returnLabel);
            }
            else if (operation.GetType() == typeof(Multiplication))
            {
                Multiplication multiplication = (Multiplication)operation;
                GenerateMethodBody(generator, multiplication.Argument1, functionRegistry);
                GenerateMethodBody(generator, multiplication.Argument2, functionRegistry);

                generator.Emit(OpCodes.Mul);
            }
            else if (operation.GetType() == typeof(Addition))
            {
                Addition addition = (Addition)operation;
                GenerateMethodBody(generator, addition.Argument1, functionRegistry);
                GenerateMethodBody(generator, addition.Argument2, functionRegistry);

                generator.Emit(OpCodes.Add);
            }
            else if (operation.GetType() == typeof(Subtraction))
            {
                Subtraction addition = (Subtraction)operation;
                GenerateMethodBody(generator, addition.Argument1, functionRegistry);
                GenerateMethodBody(generator, addition.Argument2, functionRegistry);

                generator.Emit(OpCodes.Sub);
            }
            else if (operation.GetType() == typeof(Division))
            {
                Division division = (Division)operation;
                GenerateMethodBody(generator, division.Dividend, functionRegistry);
                GenerateMethodBody(generator, division.Divisor, functionRegistry);

                generator.Emit(OpCodes.Div);
            }
            else if (operation.GetType() == typeof(Modulo))
            {
                Modulo modulo = (Modulo)operation;
                GenerateMethodBody(generator, modulo.Dividend, functionRegistry);
                GenerateMethodBody(generator, modulo.Divisor, functionRegistry);

                generator.Emit(OpCodes.Rem);
            }
            else if (operation.GetType() == typeof(Exponentiation))
            {
                Exponentiation exponentation = (Exponentiation)operation;
                GenerateMethodBody(generator, exponentation.Base, functionRegistry);
                GenerateMethodBody(generator, exponentation.Exponent, functionRegistry);

                generator.Emit(OpCodes.Call, typeof(Math).GetMethod("Pow"));
            }
            else if (operation.GetType() == typeof(UnaryMinus))
            {
                UnaryMinus unaryMinus = (UnaryMinus)operation;
                GenerateMethodBody(generator, unaryMinus.Argument, functionRegistry);

                generator.Emit(OpCodes.Neg);
            }
            else if (operation.GetType() == typeof(LessThan))
            {
                LessThan lessThan = (LessThan)operation;

                Label ifLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();

                GenerateMethodBody(generator, lessThan.Argument1, functionRegistry);
                GenerateMethodBody(generator, lessThan.Argument2, functionRegistry);

                generator.Emit(OpCodes.Blt_S, ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 0.0);
                generator.Emit(OpCodes.Br_S, endLabel);
                generator.MarkLabel(ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 1.0);
                generator.MarkLabel(endLabel);
            }
            else if (operation.GetType() == typeof(LessOrEqualThan))
            {
                LessOrEqualThan lessOrEqualThan = (LessOrEqualThan)operation;

                Label ifLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();

                GenerateMethodBody(generator, lessOrEqualThan.Argument1, functionRegistry);
                GenerateMethodBody(generator, lessOrEqualThan.Argument2, functionRegistry);

                generator.Emit(OpCodes.Ble_S, ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 0.0);
                generator.Emit(OpCodes.Br_S, endLabel);
                generator.MarkLabel(ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 1.0);
                generator.MarkLabel(endLabel);
            }
            else if (operation.GetType() == typeof(GreaterThan))
            {
                GreaterThan greaterThan = (GreaterThan)operation;

                Label ifLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();

                GenerateMethodBody(generator, greaterThan.Argument1, functionRegistry);
                GenerateMethodBody(generator, greaterThan.Argument2, functionRegistry);

                generator.Emit(OpCodes.Bgt_S, ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 0.0);
                generator.Emit(OpCodes.Br_S, endLabel);
                generator.MarkLabel(ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 1.0);
                generator.MarkLabel(endLabel);
            }
            else if (operation.GetType() == typeof(GreaterOrEqualThan))
            {
                GreaterOrEqualThan greaterOrEqualThan = (GreaterOrEqualThan)operation;

                Label ifLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();

                GenerateMethodBody(generator, greaterOrEqualThan.Argument1, functionRegistry);
                GenerateMethodBody(generator, greaterOrEqualThan.Argument2, functionRegistry);

                generator.Emit(OpCodes.Bge_S, ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 0.0);
                generator.Emit(OpCodes.Br_S, endLabel);
                generator.MarkLabel(ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 1.0);
                generator.MarkLabel(endLabel);
            }
            else if (operation.GetType() == typeof(Equal))
            {
                Equal equal = (Equal)operation;

                Label ifLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();

                GenerateMethodBody(generator, equal.Argument1, functionRegistry);
                GenerateMethodBody(generator, equal.Argument2, functionRegistry);

                generator.Emit(OpCodes.Beq_S, ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 0.0);
                generator.Emit(OpCodes.Br_S, endLabel);
                generator.MarkLabel(ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 1.0);
                generator.MarkLabel(endLabel);
            }
            else if (operation.GetType() == typeof(NotEqual))
            {
                NotEqual notEqual = (NotEqual)operation;

                Label ifLabel = generator.DefineLabel();
                Label endLabel = generator.DefineLabel();

                GenerateMethodBody(generator, notEqual.Argument1, functionRegistry);
                GenerateMethodBody(generator, notEqual.Argument2, functionRegistry);

                generator.Emit(OpCodes.Beq, ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 1.0);
                generator.Emit(OpCodes.Br_S, endLabel);
                generator.MarkLabel(ifLabel);
                generator.Emit(OpCodes.Ldc_R8, 0.0);
                generator.MarkLabel(endLabel);
            }
            else if (operation.GetType() == typeof(Function))
            {
                Function function = (Function)operation;

                FunctionInfo functionInfo = functionRegistry.GetFunctionInfo(function.FunctionName);
                Type funcType = GetFuncType(functionInfo.NumberOfParameters);

                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, typeof(FormulaContext<double>).GetProperty("FunctionRegistry").GetGetMethod());
                generator.Emit(OpCodes.Ldstr, function.FunctionName);
                generator.Emit(OpCodes.Callvirt, typeof(IFunctionRegistry).GetMethod("GetFunctionInfo", new Type[] { typeof(string) }));
                generator.Emit(OpCodes.Callvirt, typeof(FunctionInfo).GetProperty("Function").GetGetMethod());
                generator.Emit(OpCodes.Castclass, funcType);

                for (int i = 0; i < functionInfo.NumberOfParameters; i++)
                    GenerateMethodBody(generator, function.Arguments[i], functionRegistry);

                generator.Emit(OpCodes.Call, funcType.GetMethod("Invoke"));
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
                typeArguments[i] = typeof(double);

            return funcType.MakeGenericType(typeArguments);
        }
    }
#endif
}
