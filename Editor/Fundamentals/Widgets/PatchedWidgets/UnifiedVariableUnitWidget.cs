using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public sealed class UnifiedVariableUnitWidget : UnitWidget<UnifiedVariableUnit>
    {
        public UnifiedVariableUnitWidget(FlowCanvas canvas, UnifiedVariableUnit unit) : base(canvas, unit)
        {
            nameInspectorConstructor = (metadata) => new VariableNameInspector(metadata, GetNameSuggestions);
        }

        protected override NodeColorMix baseColor => NodeColorMix.TealReadable;

        private VariableNameInspector nameInspector;
        private Func<Metadata, VariableNameInspector> nameInspectorConstructor;

        public override Inspector GetPortInspector(IUnitPort port, Metadata metadata)
        {
            if (port == unit.name)
            {
                // This feels so hacky. The real holy grail here would be to support attribute decorators like Unity does.
                InspectorProvider.instance.Renew(ref nameInspector, metadata, nameInspectorConstructor);

                return nameInspector;
            }

            return base.GetPortInspector(port, metadata);
        }

        protected override IEnumerable<DropdownOption> contextOptions
        {
            get
            {
                foreach (var option in base.contextOptions)
                {
                    yield return option;
                }

                if (!unit.name.hasValidConnection && !Flow.CanPredict(unit.name, reference))
                    yield break;

                yield return new DropdownOption((Action)FindAll, "Find/All");
                yield return new DropdownOption((Action)FindSetters, "Find/Setters");
                yield return new DropdownOption((Action)FindGetters, "Find/Getters");
            }
        }

        private void FindAll()
        {
            var name = Flow.Predict<string>(unit.name, reference);
            NodeFinderWindow.Open($"{name} [SetVariable: {unit.kind}] | {name} [GetVariable: {unit.kind}]");
        }

        private void FindSetters()
        {
            var name = Flow.Predict<string>(unit.name, reference);
            NodeFinderWindow.Open($"{name} [SetVariable: {unit.kind}]");
        }

        private void FindGetters()
        {
            var name = Flow.Predict<string>(unit.name, reference);
            NodeFinderWindow.Open($"{name} [GetVariable: {unit.kind}]");
        }

        private IEnumerable<string> GetNameSuggestions()
        {
            return EditorVariablesUtility.GetVariableNameSuggestions(unit.kind, reference);
        }
    }
}
