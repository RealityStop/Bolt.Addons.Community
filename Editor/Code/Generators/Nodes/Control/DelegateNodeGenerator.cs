using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(DelegateNode))]
    public class DelegateNodeGenerator : NodeGenerator<DelegateNode>
    {
        public DelegateNodeGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (output == Unit.@delegate || output == Unit.Callback)
            {
                var delegateType = Unit._delegate.GetDelegateType();

                data.CreateSymbol(Unit, delegateType);
                data.NewScope();

                var parameters = new List<string>();

                if (Unit._delegate is IFunc func)
                {
                    data.SetReturns(func.ReturnType);

                    var args = delegateType.GetGenericArguments();
                    for (int i = 0; i < args.Length - 1; i++)
                    {
                        parameters.Add(data.AddLocalNameInScope("arg" + i, args[i]).VariableHighlight());
                    }
                }
                else
                {
                    var action = Unit._delegate as IAction;
                    data.SetReturns(typeof(void));

                    var args = delegateType.GetGenericArguments();
                    for (int i = 0; i < args.Length; i++)
                    {
                        parameters.Add(data.AddLocalNameInScope("arg" + i, args[i]).VariableHighlight());
                    }
                }

                writer.MultilineLambda(writer.Action(() =>
                {
                    GenerateControl(null, data, writer);
                    if (Unit._delegate is IFunc)
                    {
                        writer.Return(writer.Action(() => GenerateValue((Unit as FuncNode).@return, data, writer)), WriteOptions.IndentedNewLineAfter);
                    }
                }), parameters.Select(p => (CodeWriter.MethodParameter)p).ToArray());

                data.ExitScope();
                return;
            }

            if (Unit.parameters.Contains(output) && (Unit.@delegate.hasValidConnection || Unit.Callback.hasValidConnection))
            {
                writer.Write(data.GetVariableName("arg" + Unit.parameters.IndexOf(output)).VariableHighlight());
                return;
            }
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            GenerateChildControl(Unit.invoke, data, writer);
        }
    }
}