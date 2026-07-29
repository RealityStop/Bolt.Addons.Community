using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class FlowGraphEditor : GraphEditor
    {
        public FlowGraphEditor(Metadata metadata) : base(metadata)
        {
            graph.units.ItemAdded += OnUnitAdded;
            graph.units.ItemRemoved += OnItemRemoved;

            nesterMetadata = Metadata.Root().Object("editor", this).Member("nesters", BindingFlags.Instance | BindingFlags.NonPublic);

            nesterMetadata.value = new List<IGraphNesterElement>();
            IEnumerable<IGraphNesterElement> nesterUnits = graph.units.OfType<IGraphNesterElement>();

            nesterMetadata.value = nesterUnits
            .Where(nester => nester.nest.source == GraphSource.Embed || nester == nesterUnits
            .FirstOrDefault(n => n.nest.source == GraphSource.Macro && n.nest.macro == nester.nest.macro)).ToList();

            nesterAdaptor = new NesterAdaptor(nesterMetadata, this);
        }

        private bool nestersCached;

        [InspectorWide(true)]
        [UsedImplicitly]
        private List<IGraphNesterElement> nesters = new List<IGraphNesterElement>();

        private NesterAdaptor nesterAdaptor;

        private void OnItemRemoved(IUnit unit)
        {
            if (unit is INesterUnit) nestersCached = false;
        }

        private void OnUnitAdded(IUnit unit)
        {
            if (unit is INesterUnit) nestersCached = false;
        }

        public override void Dispose()
        {
            graph.units.ItemAdded -= OnUnitAdded;
            graph.units.ItemRemoved -= OnItemRemoved;
        }

        private new FlowGraph graph => (FlowGraph)base.graph;

        private Metadata controlInputDefinitionsMetadata => metadata[nameof(FlowGraph.controlInputDefinitions)];
        private Metadata controlOutputDefinitionsMetadata => metadata[nameof(FlowGraph.controlOutputDefinitions)];
        private Metadata valueInputDefinitionsMetadata => metadata[nameof(FlowGraph.valueInputDefinitions)];
        private Metadata valueOutputDefinitionsMetadata => metadata[nameof(FlowGraph.valueOutputDefinitions)];

        private readonly Metadata nesterMetadata;

        private IEnumerable<Warning> warnings => UnitPortDefinitionUtility.Warnings((FlowGraph)metadata.value);

        protected override float GetHeight(float width, GUIContent label)
        {
            var height = 0f;

            height += GetHeaderHeight(width);

            height += GetControlInputDefinitionsHeight(width);

            height += EditorGUIUtility.standardVerticalSpacing;

            height += GetControlOutputDefinitionsHeight(width);

            height += EditorGUIUtility.standardVerticalSpacing;

            height += GetValueInputDefinitionsHeight(width);

            height += EditorGUIUtility.standardVerticalSpacing;

            height += GetValueOutputDefinitionsHeight(width);

            height += EditorGUIUtility.standardVerticalSpacing;

            height += LudiqGUI.GetInspectorHeight(this, nesterMetadata, width);

            height += EditorGUIUtility.standardVerticalSpacing;

            if (warnings.Any())
            {
                height += EditorGUIUtility.standardVerticalSpacing;

                foreach (var warning in warnings)
                {
                    height += warning.GetHeight(width) + 1f;
                }
            }

            return height;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (!nestersCached)
            {
                IEnumerable<IGraphNesterElement> nesterUnits = graph.units.OfType<IGraphNesterElement>();

                nesterMetadata.value = nesterUnits
                .Where(nester => nester.nest.source == GraphSource.Embed || nester == nesterUnits
                .FirstOrDefault(n => n.nest.source == GraphSource.Macro && n.nest.macro == nester.nest.macro)).ToList();
                nestersCached = true;
            }

            BeginLabeledBlock(metadata, position, label);

            OnHeaderGUI(position);

            EditorGUI.BeginChangeCheck();

            LudiqGUI.Inspector(controlInputDefinitionsMetadata, position.VerticalSection(ref y, GetControlInputDefinitionsHeight(position.width)));

            y += EditorGUIUtility.standardVerticalSpacing;

            LudiqGUI.Inspector(controlOutputDefinitionsMetadata, position.VerticalSection(ref y, GetControlOutputDefinitionsHeight(position.width)));

            y += EditorGUIUtility.standardVerticalSpacing;

            LudiqGUI.Inspector(valueInputDefinitionsMetadata, position.VerticalSection(ref y, GetValueInputDefinitionsHeight(position.width)));

            y += EditorGUIUtility.standardVerticalSpacing;

            LudiqGUI.Inspector(valueOutputDefinitionsMetadata, position.VerticalSection(ref y, GetValueOutputDefinitionsHeight(position.width)));

            if (EditorGUI.EndChangeCheck())
            {
                graph.PortDefinitionsChanged();
            }

            y += EditorGUIUtility.standardVerticalSpacing;

            nesterAdaptor.Field(position.VerticalSection(ref y, nesterAdaptor.GetHeight(position.width, new GUIContent("Graphs"))), new GUIContent("Graphs"));

            if (warnings.Any())
            {
                y += EditorGUIUtility.standardVerticalSpacing;

                foreach (var warning in warnings)
                {
                    y--;
                    warning.OnGUI(position.VerticalSection(ref y, warning.GetHeight(position.width) + 1));
                }
            }

            EndBlock(metadata);
        }

        private float GetControlInputDefinitionsHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(this, controlInputDefinitionsMetadata, width);
        }

        private float GetControlOutputDefinitionsHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(this, controlOutputDefinitionsMetadata, width);
        }

        private float GetValueInputDefinitionsHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(this, valueInputDefinitionsMetadata, width);
        }

        private float GetValueOutputDefinitionsHeight(float width)
        {
            return LudiqGUI.GetInspectorHeight(this, valueOutputDefinitionsMetadata, width);
        }
    }
}
