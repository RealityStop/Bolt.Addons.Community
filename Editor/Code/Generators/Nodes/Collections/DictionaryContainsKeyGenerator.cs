using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community.Libraries.CSharp;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(DictionaryContainsKey))]
    public class DictionaryContainsKeyGenerator : NodeGenerator<DictionaryContainsKey>
    {
        public DictionaryContainsKeyGenerator(Unit unit) : base(unit) { }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            writer.InvokeMember(
                writer.Action(w => GenerateValue(Unit.dictionary, data, w)),
                "Contains",
                writer.Action(w => GenerateValue(Unit.key, data, w))
            );
        }
    }
}