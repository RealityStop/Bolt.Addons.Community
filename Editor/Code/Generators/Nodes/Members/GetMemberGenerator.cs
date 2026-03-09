using Unity;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using System.Linq;
using Unity.VisualScripting.Community.Libraries.CSharp;
using System.Collections.Generic;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;
using System;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(GetMember))]
    public sealed class GetMemberGenerator : NodeGenerator<GetMember>
    {
        public GetMemberGenerator(GetMember unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return Unit.member.pseudoDeclaringType.Namespace;
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            data.CreateSymbol(Unit, output.type);

            if (Unit.target == null)
            {
                writer.GetMember(Unit.member.targetType, Unit.member.name);
                return;
            }

            if (!Unit.target.hasValidConnection)
            {
                writer.GetMember(writer.Action(writer => GenerateValue(Unit.target, data, writer)), Unit.member.name);
                return;
            }

            string name;

            if (Unit.member.isField)
            {
                name = Unit.member.fieldInfo.Name;
            }
            else if (Unit.member.isProperty)
            {
                name = Unit.member.name;
            }
            else
            {
                name = Unit.member.ToDeclarer().ToString(); // I don't think this should be possible through normal usage.
            }

            if (!typeof(Component).IsAssignableFrom(Unit.member.pseudoDeclaringType))
            {
                writer.GetMember(writer.Action(writer => GenerateValue(Unit.target, data, writer)), name);
                return;
            }
            ExpectedTypeResult result;
            CaptureResult captureResult;
            using (data.Expect(Unit.target.type, out result))
            {
                using (writer.Capture(out captureResult))
                {
                    GenerateValue(Unit.target, data, writer);
                }
            }

            var targetCode = captureResult.Value;
            var code = !result.IsSatisfied ? Unit.target.GetComponent(writer, SourceType(Unit.target, data, writer), Unit.member.pseudoDeclaringType, true, true) : "";
            if (!string.IsNullOrEmpty(code))
            {
                writer.Write(targetCode).Write(code).GetMember(name);
                return;
            }
            else
            {
                writer.GetMember(writer.Action(w => w.Write(targetCode)), name);
                return;
            }
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (Unit.target != null)
            {
                if (input == Unit.target)
                {
                    if (Unit.target.hasValidConnection)
                    {
                        GenerateConnectedValue(input, data, writer);
                        if (ShouldCast(input, data, writer))
                        {
                            var expected = data.GetExpectedType();
                            if (expected != null && expected.IsConvertibleTo(input.type, true))
                            {
                                data.MarkExpectedTypeMet(input.type);
                            }
                            writer.WriteConvertTo(input.type, true);
                        }
                        // if (typeof(Component).IsStrictlyAssignableFrom(Unit.member.pseudoDeclaringType))
                        // {
                        //     return connectedCode.CastAs(typeof(GameObject), Unit, ShouldCast(input, data));
                        // }
                        return;
                    }
                    else if (Unit.target.hasDefaultValue)
                    {
                        if (input.type == typeof(GameObject) || typeof(Component).IsStrictlyAssignableFrom(input.type))
                        {
                            var sourceType = SourceType(Unit.target, data, writer);
                            var code = Unit.target.GetComponent(writer, sourceType, Unit.member.pseudoDeclaringType, true, true);
                            if (!string.IsNullOrEmpty(code))
                            {
                                writer.GetVariable("gameObject").Write(code);
                                return;
                            }
                            else
                            {
                                writer.GetVariable("gameObject");
                                return;
                            }
                        }
                        base.GenerateValueInternal(input, data, writer);
                    }
                }
            }

            base.GenerateValueInternal(input, data, writer);
        }

        private Type SourceType(ValueInput valueInput, ControlGenerationData data, CodeWriter writer)
        {
            return GetSourceType(valueInput, data, writer) ?? valueInput.connection?.source?.type ?? valueInput.type;
        }
    }
}