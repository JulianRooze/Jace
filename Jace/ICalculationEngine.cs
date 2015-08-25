using Jace.Execution;
using System;
namespace Jace
{
  public interface ICalculationEngine<T>
  {
    void AddConstant(string constantName, T value);
    void AddFunction(string functionName, Func<T, T, T, T, T, T, T> function);
    void AddFunction(string functionName, Func<T, T, T, T, T, T> function);
    void AddFunction(string functionName, Func<T, T, T, T, T> function);
    void AddFunction(string functionName, Func<T, T, T, T> function);
    void AddFunction(string functionName, Func<T, T, T> function);
    void AddFunction(string functionName, Func<T, T> function);
    void AddFunction(string functionName, Func<T> function);
    Func<System.Collections.Generic.Dictionary<string, T>, T> Build(string formulaText);
    T Calculate(string formulaText);
    T Calculate(string formulaText, System.Collections.Generic.IDictionary<string, T> variables);
    Jace.Execution.FormulaBuilder<T> Formula(string formulaText);
    IFunctionRegistry FunctionRegistry { get; }

  }
}
