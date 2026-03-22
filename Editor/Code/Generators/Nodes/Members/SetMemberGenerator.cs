using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(SetMember))]
    public sealed class SetMemberGenerator : NodeGenerator<SetMember>
    {
        public SetMemberGenerator(SetMember unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            yield return Unit.member.pseudoDeclaringType.Namespace;
        }

        protected override void GenerateControlInternal(ControlInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input == Unit.assign)
            {
                var memberName = Unit.member.name.VariableHighlight();

                if (Unit.target != null)
                {
                    ExpectedTypeResult result;
                    using (data.Expect(Unit.target.type, out result))
                    {
                        writer.WriteIndented();
                        GenerateValue(Unit.target, data, writer);
                    }

                    writer.GetMember(null, memberName).Equal();
                    using (data.Expect(Unit.input.type))
                    {
                        GenerateValue(Unit.input, data, writer);
                    }
                    writer.Write(";").NewLine();
                }
                else
                {
                    writer.GetMember(Unit.member.pseudoDeclaringType.As().CSharpName(false, true), memberName).Equal();
                    using (data.Expect(Unit.input.type))
                    {
                        GenerateValue(Unit.input, data, writer);
                    }
                    writer.Write(";").NewLine();
                }
                GenerateExitControl(Unit.assigned, data, writer);
            }
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            ExpectedTypeResult result;
            using (data.Expect(Unit.target.type, out result))
            {
                GenerateValue(Unit.target, data, writer);
            }
            writer.GetMember(Unit.member.name);
        }

        protected override void GenerateValueInternal(ValueInput input, ControlGenerationData data, CodeWriter writer)
        {
            if (input != Unit.target)
            {
                base.GenerateValueInternal(input, data, writer);
            }
            else
            {
                if (input.hasValidConnection)
                {
                    GenerateConnectedValueCasted(input, data, writer, input.type, () => ShouldCast(input, data, writer), true);
                }
                else if (Unit.target.hasDefaultValue)
                {
                    if (unit.defaultValues[Unit.target.key] == null)
                    {
                        if (input.type == typeof(GameObject))
                        {
                            writer.GetVariable("gameObject");
                            return;
                        }
                        else if (typeof(Component).IsStrictlyAssignableFrom(input.type))
                        {
                            writer.GetVariable("gameObject").Write(input.GetComponent(writer, GetSourceType(input, data, writer, true, true), input.type, true, true));
                            return;
                        }
                    }
                    base.GenerateValueInternal(input, data, writer);
                }
                else
                {
                    base.GenerateValueInternal(input, data, writer);
                }
            }
        }
    }
}