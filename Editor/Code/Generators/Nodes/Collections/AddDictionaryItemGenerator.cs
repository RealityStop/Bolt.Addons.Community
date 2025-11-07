#pragma warning disable
using Unity.VisualScripting.Community;
using Unity.VisualScripting;
using System;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(AddDictionaryItem))]
    [RenamedFrom("Unity.VisualScrripting.Community.AddDictionaryItemGenerator")]// Typo VisualScrripting
    public class AddDictionaryItemGenerator : NodeGenerator<AddDictionaryItem>
    {
        public AddDictionaryItemGenerator(Unit unit) : base(unit)
        {
        }

        public override string GenerateControl(ControlInput input, ControlGenerationData data, int indent)
        {
            string output = string.Empty;
            var keyCode = "";
            var valueCode = "";
            data.SetExpectedType(typeof(System.Collections.IDictionary));
            output = output + Unity.VisualScripting.Community.Libraries.CSharp.CodeBuilder.Indent(indent) + base.GenerateValue(this.Unit.dictionaryInput, data) + MakeClickableForThisUnit(".Add(", true);
            var result = data.RemoveExpectedType();
            if (result.isMet && typeof(System.Collections.IDictionary).IsAssignableFrom(result.type))
            {
                data.SetExpectedType(GetKeyExpectedType(result.type));
                keyCode = base.GenerateValue(this.Unit.key, data);
                data.RemoveExpectedType();

                data.SetExpectedType(GetValueExpectedType(result.type));
                valueCode = base.GenerateValue(this.Unit.value, data);
                data.RemoveExpectedType();
            }
            else
            {
                keyCode = base.GenerateValue(this.Unit.key, data);
                valueCode = base.GenerateValue(this.Unit.value, data);
            }

            output = output + keyCode + MakeClickableForThisUnit(", ", true) + valueCode + MakeClickableForThisUnit(");", true) + "\n" + GetNextUnit(this.Unit.exit, data, indent);
            return output;
        }

        public override string GenerateValue(ValueOutput output, ControlGenerationData data)
        {
            return base.GenerateValue(this.Unit.dictionaryInput, data);
        }

        public Type GetKeyExpectedType(Type type)
        {
            if (type.IsGenericType)
            {
                return type.GetGenericArguments()[0];
            }

            return typeof(object);
        }

        public Type GetValueExpectedType(Type type)
        {
            if (type.IsGenericType && type.GetGenericArguments().Length > 1)
            {
                return type.GetGenericArguments()[1];
            }

            return typeof(object);
        }
    }
}