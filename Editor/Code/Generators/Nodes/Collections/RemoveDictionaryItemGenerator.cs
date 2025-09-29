
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting;

namespace Unity.VisualScripting.Community
{
    [NodeGenerator(typeof(RemoveDictionaryItem))]
    public class RemoveDictionaryItemGenerator : NodeGenerator<RemoveDictionaryItem>
    {
        public RemoveDictionaryItemGenerator(Unit unit) : base(unit) { }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(output, data);
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            data.SetExpectedType(typeof(System.Collections.IDictionary));
            string output = CodeBuilder.Indent(indent) + GenerateValue(Unit.dictionaryInput, data) + MakeClickableForThisUnit(".Remove(");
            var (type, isMet) = data.RemoveExpectedType();
            string keyCode;

            if (isMet && typeof(System.Collections.IDictionary).IsAssignableFrom(type))
            {
                data.SetExpectedType(GetKeyExpectedType(type));
                keyCode = base.GenerateValue(Unit.key, data);
                data.RemoveExpectedType();
            }
            else
            {
                keyCode = base.GenerateValue(Unit.key, data);
            }

            output = output + keyCode + MakeClickableForThisUnit(");") + "\n" + GetNextUnit(Unit.exit, data, indent);
            return output;
        }

        public Type GetKeyExpectedType(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return typeof(object);
        }

    }
}
