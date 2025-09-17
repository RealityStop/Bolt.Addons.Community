using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;

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
                if (data.MustReturn)
                {
                    var sourceType = GetSourceType(Unit.value, data) ?? typeof(object);
                    data.SetHasReturned(data.Returns == sourceType || data.Returns.IsAssignableFrom(sourceType));
                }
                var yield = SupportsYieldReturn(data.Returns) ? "yield ".ControlHighlight() : "";
                output += CodeBuilder.Indent(indent) + MakeClickableForThisUnit(yield + "return".ControlHighlight()) + (!data.Returns.Is().Void() ? MakeClickableForThisUnit(" ") + GenerateValue(Unit.value, data) : "") + MakeClickableForThisUnit(";");
                return output;
            }
            return base.GenerateControl(input, data, indent);
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
