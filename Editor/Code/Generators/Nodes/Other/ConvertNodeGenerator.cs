using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using UnityEngine;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.Humility;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(ConvertNode))]
    public class ConvertNodeGenerator : NodeGenerator<ConvertNode>
    {
        public ConvertNodeGenerator(Unit unit) : base(unit) { }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.conversion == ConversionType.Any && Unit.type != typeof(object))
            {
                var @namespace = Unit.type.Namespace;
                if (!string.IsNullOrEmpty(@namespace))
                yield return @namespace;
            }
            else
            {
                yield return "System.Linq";
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            if (data.GetExpectedType() != null && data.GetExpectedType() == Unit.type)
            {
                data.MarkExpectedTypeMet(Unit.type);
            }

            switch (Unit.conversion)
            {
                case ConversionType.Any:
                    GenerateValue(Unit.value, data, writer);
                    break;

                case ConversionType.ToArrayOfObject:
                    GenerateValue(Unit.value, data, writer);
                    writer.InvokeMember(null, $"Cast<{"object".ConstructHighlight()}>().ToArray()");
                    break;

                case ConversionType.ToListOfObject:
                    GenerateValue(Unit.value, data, writer);
                    writer.InvokeMember(null, $"Cast<{"object".ConstructHighlight()}>().ToList()");
                    break;

                default:
                    GenerateValue(Unit.value, data, writer);
                    break;
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input.hasInvalidConnection)
            {
                Type castType = Unit.type;
                if (Unit.conversion != ConversionType.Any)
                {
                    castType = typeof(IEnumerable);
                }
                GenerateConnectedValueCasted(input, data, writer, castType, () => Unit.type != typeof(object) && ShouldCast(Unit.value, data, writer), true);
                return;
            }
            base.GenerateValueInternal(input, data, writer);
        }
    }
}