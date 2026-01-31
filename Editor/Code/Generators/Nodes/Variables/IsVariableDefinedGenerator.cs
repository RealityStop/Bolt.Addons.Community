using System.Collections.Generic;
using Unity.VisualScripting;
using Unity.VisualScripting.Community;
using Unity.VisualScripting.Community.Libraries.CSharp;
using Unity.VisualScripting.Community.Libraries.Humility;
using UnityEngine;

namespace Unity.VisualScripting.Community.CSharp
{
    [NodeGenerator(typeof(IsVariableDefined))]
    public class IsVariableDefinedGenerator : LocalVariableGenerator
    {
        private IsVariableDefined Unit => unit as IsVariableDefined;
        public IsVariableDefinedGenerator(Unit unit) : base(unit)
        {
        }

        public override IEnumerable<string> GetNamespaces()
        {
            if (Unit.kind == VariableKind.Scene)
                yield return "UnityEngine.SceneManagement";
        }

        protected override void GenerateValueInternal(ValueOutput output, ControlGenerationData data, CodeWriter writer)
        {
            switch (Unit.kind)
            {
                case VariableKind.Flow:
                case VariableKind.Graph:
                    writer.Error($"{Unit.kind} Variables do not support connected names");
                    break;
                case VariableKind.Object:
                    writer.InvokeMember(writer.GetTypeNameHighlighted(typeof(VisualScripting.Variables)), "Object", writer.Action(() => GenerateValue(Unit.@object, data, writer)));
                    break;
                case VariableKind.Scene:
                    writer.Write(writer.GetTypeNameHighlighted(typeof(VisualScripting.Variables)));
                    WriteSceneKind(data, writer);
                    break;
                case VariableKind.Application:
                    writer.GetMember(writer.GetTypeNameHighlighted(typeof(VisualScripting.Variables)), "Application");
                    break;
                case VariableKind.Saved:
                    writer.GetMember(writer.GetTypeNameHighlighted(typeof(VisualScripting.Variables)), "Saved");
                    break;
            }
            writer.InvokeMember(null, "IsDefined", writer.Action(() => GenerateValue(Unit.name, data, writer)));
        }

        private void WriteSceneKind(ControlGenerationData data, CodeWriter writer)
        {
            writer.Write(typeof(Component).IsAssignableFrom(data.ScriptType)
                ? ".Scene(" + "gameObject".VariableHighlight() + "." + "scene".VariableHighlight() + ")"
                : "." + "ActiveScene".VariableHighlight());
        }
    }
}