using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(EventReturn))]
    public class ReturnGenerator : NodeGenerator<EventReturn>
    {
        public ReturnGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.enter)
            {
                if (Unit.data.hasValidConnection)
                {
                    writer.WriteIndented();
                    using (data.Expect(typeof(ReturnEventArg)))
                        GenerateValue(Unit.data, data, writer);
                    writer.Write($".{"callback".VariableHighlight()}?.Invoke(");
                    GenerateValue(Unit.value, data, writer);
                    writer.WriteEnd();
                    return;
                }

                if (data.MustReturn)
                {
                    var sourceType = GetSourceType(Unit.value, data, writer);
                    data.SetHasReturned(data.Returns == sourceType || data.Returns.IsAssignableFrom(sourceType));
                }
                else if (data.MustBreak)
                {
                    data.SetHasBroke(true);
                }

                writer.WriteIndented();
                bool isYieldReturn = SupportsYieldReturn(data.Returns);
                if (isYieldReturn)
                {
                    writer.Write("yield ".ControlHighlight());
                }
                writer.Write("return".ControlHighlight());

                if (!data.Returns.Is().Void())
                {
                    writer.Write(" ");
                    GenerateValue(Unit.value, data, writer);
                }

                writer.WriteEnd(EndWriteOptions.LineEnd);
            }
        }

        /// <summary>
        /// Checks if a type can be used with `yield return`.
        /// </summary>
        private bool SupportsYieldReturn(Type type)
        {
            return type != null && (type == typeof(IEnumerable) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || type == typeof(IEnumerator) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerator<>)));
        }
    }
}
