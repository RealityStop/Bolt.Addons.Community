using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(EventReturn))]
    public class ReturnGenerator : NodeGenerator<EventReturn>
    {
        public ReturnGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string output = string.Empty;
            if (input == Unit.enter)
            {
                data.hasReturned = true;
                var yield = SupportsYieldReturn(data.returns) ? "yield ".ControlHighlight() : "";
                output += CodeBuilder.Indent(indent) + MakeSelectableForThisUnit(yield + "return".ControlHighlight()) + (data.returns != typeof(void) && data.returns != typeof(Libraries.CSharp.Void) ? " " + GenerateValue(Unit.value, data) : "") + MakeSelectableForThisUnit(";");
                return output;
            }
            return base.GenerateControl(input, data, indent);
        }

        /// <summary>
        /// Checks if a type can be used with `yield return`.
        /// </summary>
        private bool SupportsYieldReturn(Type type)
        {
            // Only check if the type is exactly IEnumerable or IEnumerable<T> (not concrete implementations)
            return type != null && (type == typeof(IEnumerable) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || type == typeof(IEnumerator) || (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerator<>)));
        }
    }
}
