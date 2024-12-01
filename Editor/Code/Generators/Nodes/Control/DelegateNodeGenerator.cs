
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(DelegateNode))]
    public class DelegateNodeGenerator : NodeGenerator<DelegateNode>
    {
        public DelegateNodeGenerator(Unit unit) : base(unit) { }
        ControlGenerationData _data;
        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            if (output == Unit.@delegate || output == Unit.Callback)
            {
                _data = new ControlGenerationData(data);
                _data.NewScope();
                List<string> parameters = new List<string>();
                var index = 0;
                if (Unit._delegate is IFunc func)
                {
                    _data.returns = func.ReturnType;
                    foreach (var type in func.GetDelegateType().GetGenericArguments())
                    {
                        if (type != func.GetDelegateType().GetGenericArguments().Last())
                        {
                            parameters.Add(_data.AddLocalNameInScope("arg" + index, Unit._delegate.GetDelegateType()).VariableHighlight());
                            index++;
                        }
                    }
                }
                else
                {
                    var action = Unit._delegate as IAction;
                    _data.returns = typeof(void);
                    foreach (var type in action.GetDelegateType().GetGenericArguments())
                    {
                        parameters.Add(_data.AddLocalNameInScope("arg" + index, Unit._delegate.GetDelegateType()).VariableHighlight());
                        index++;
                    }
                }
                var delegateCode = CodeBuilder.MultiLineLambda(Unit, MakeSelectableForThisUnit(string.Join(", ", parameters)), GenerateControl(null, _data, CodeBuilder.currentIndent) + (Unit._delegate is IFunc ? "\n" + CodeBuilder.GetCurrentIndent(Unit.invoke.hasValidConnection ? 0 : 1) + MakeSelectableForThisUnit("return ".ControlHighlight()) + GenerateValue((Unit as FuncNode).@return, _data) : string.Empty), Unit.invoke.hasValidConnection ? CodeBuilder.currentIndent - 1 : (Unit._delegate is IFunc ? CodeBuilder.currentIndent - 1 : CodeBuilder.currentIndent));
                _data.ExitScope();
                return delegateCode;
            }
            else if (Unit.parameters.Contains(output) && (Unit.@delegate.hasValidConnection || Unit.Callback.hasValidConnection))
            {
                return MakeSelectableForThisUnit(_data.GetVariableName("arg" + Unit.parameters.IndexOf(output)).VariableHighlight());
            }
            else return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            return GetNextUnit(Unit.invoke, data, indent + 1);
        }
    }
}