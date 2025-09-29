
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DelegateNode))]
    public class DelegateNodeGenerator : NodeGenerator<DelegateNode>
    {
        public DelegateNodeGenerator(Unit unit) : base(unit) { }
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.@delegate || output == Unit.Callback)
            {
                data.CreateSymbol(Unit, Unit._delegate.GetDelegateType());
                data.NewScope();
                List<string> parameters = new List<string>();
                for (int i = 0; i < Unit._delegate.GetDelegateType().GetGenericArguments().Length; i++)
                {
                    if (Unit._delegate is IFunc func)
                    {
                        data.SetReturns(func.ReturnType);
                        foreach (var type in func.GetDelegateType().GetGenericArguments())
                        {
                            if (i < func.GetDelegateType().GetGenericArguments().Length - 1)
                            {
                                parameters.Add(data.AddLocalNameInScope("arg" + i, Unit._delegate.GetDelegateType().GetGenericArguments()[i]).VariableHighlight());
                            }
                        }
                    }
                    else
                    {
                        var action = Unit._delegate as IAction;
                        data.SetReturns(typeof(void));
                        foreach (var type in action.GetDelegateType().GetGenericArguments())
                        {
                            parameters.Add(data.AddLocalNameInScope("arg" + i, Unit._delegate.GetDelegateType().GetGenericArguments()[i]).VariableHighlight());
                        }
                    }
                }
                var delegateCode = CodeBuilder.MultiLineLambda(Unit, MakeClickableForThisUnit(string.Join(", ", parameters)), GenerateControl(null, data, CodeBuilder.currentIndent) + (Unit._delegate is IFunc ? "\n" + CodeBuilder.GetCurrentIndent(Unit.invoke.hasValidConnection ? 0 : 1) + MakeClickableForThisUnit("return ".ControlHighlight()) + GenerateValue((Unit as FuncNode).@return, data) + MakeClickableForThisUnit(";") : string.Empty), Unit.invoke.hasValidConnection ? CodeBuilder.currentIndent - 1 : (Unit._delegate is IFunc ? CodeBuilder.currentIndent - 1 : CodeBuilder.currentIndent));
                data.ExitScope();
                return delegateCode;
            }
            else if (Unit.parameters.Contains(output) && (Unit.@delegate.hasValidConnection || Unit.Callback.hasValidConnection))
            {
                return MakeClickableForThisUnit(data.GetVariableName("arg" + Unit.parameters.IndexOf(output)).VariableHighlight());
            }
            else return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.invoke, data, indent + 1).TrimEnd();
        }
    }
}