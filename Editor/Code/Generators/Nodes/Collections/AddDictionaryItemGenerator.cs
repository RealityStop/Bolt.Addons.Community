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
    [RenamedFrom("Unity.VisualScrripting.Community.AddDictionaryItemGenerator")]
    public class AddDictionaryItemGenerator : NodeGenerator<AddDictionaryItem>
    {
        public AddDictionaryItemGenerator(Unit unit) : base(unit)
        {
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            ExpectedTypeResult result;
            using (data.Expect(typeof(System.Collections.IDictionary), out result))
            {
                writer.WriteIndented();
                GenerateValue(Unit.dictionaryInput, data, writer);
            }

            writer.Write(".Add(");

            if (result.IsSatisfied && typeof(System.Collections.IDictionary).IsAssignableFrom(result.ResolvedType))
            {
                using (data.Expect(GetKeyExpectedType(result.ResolvedType)))
                {
                    GenerateValue(Unit.key, data, writer);
                }

                writer.ParameterSeparator();

                using (data.Expect(GetValueExpectedType(result.ResolvedType)))
                {
                    GenerateValue(Unit.value, data, writer);
                }
            }
            else
            {
                GenerateValue(Unit.key, data, writer);
                writer.ParameterSeparator();
                GenerateValue(Unit.value, data, writer);
            }

            writer.WriteEnd();
            GenerateExitControl(Unit.exit, data, writer);
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            GenerateValue(Unit.dictionaryInput, data, writer);
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