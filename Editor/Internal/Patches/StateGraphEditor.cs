using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using JetBrains.Annotations;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    public class PatchedStateGraphEditor : GraphEditor
    {
        public PatchedStateGraphEditor(Metadata metadata) : base(metadata)
        {
            graph.elements.ItemAdded += OnUnitAdded;
            graph.elements.ItemRemoved += OnItemRemoved;

            nesterMetadata = Metadata.Root().Object("editor", this).Member("nesters", BindingFlags.Instance | BindingFlags.NonPublic);

            nesterMetadata.value = null;
            IEnumerable<IGraphNesterElement> nesterUnits = graph.elements.OfType<IGraphNesterElement>();

            nesterMetadata.value = nesterUnits
            .Where(nester => nester.nest.source == GraphSource.Embed || nester == nesterUnits
            .FirstOrDefault(n => n.nest.source == GraphSource.Macro && n.nest.macro == nester.nest.macro)).ToList();

            nesterAdaptor = new NesterAdaptor(nesterMetadata, this);
        }

        private new StateGraph graph => (StateGraph)base.graph;

        private bool nestersCached;

        [InspectorWide(true)]
        [UsedImplicitly]
        private List<IGraphNesterElement> nesters = new List<IGraphNesterElement>();

        private NesterAdaptor nesterAdaptor;

        private readonly Metadata nesterMetadata;

        private void OnItemRemoved(IGraphElement unit)
        {
            if (unit is IGraphNesterElement) nestersCached = false;
        }

        private void OnUnitAdded(IGraphElement unit)
        {
            if (unit is IGraphNesterElement) nestersCached = false;
        }

        public override void Dispose()
        {
            graph.elements.ItemAdded -= OnUnitAdded;
            graph.elements.ItemRemoved -= OnItemRemoved;
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            var height = base.GetHeight(width, label);

            height += EditorGUIUtility.standardVerticalSpacing;

            height += LudiqGUI.GetInspectorHeight(this, nesterMetadata, width);

            return height;
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            base.OnGUI(position, label);

            if (!nestersCached)
            {
                IEnumerable<IGraphNesterElement> nesterUnits = graph.elements.OfType<IGraphNesterElement>();

                nesterMetadata.value = nesterUnits
                .Where(nester => nester.nest.source == GraphSource.Embed || nester == nesterUnits
                .FirstOrDefault(n => n.nest.source == GraphSource.Macro && n.nest.macro == nester.nest.macro)).ToList();
                nestersCached = true;
            }

            y += EditorGUIUtility.standardVerticalSpacing;

            nesterAdaptor.Field(position.VerticalSection(ref y, nesterAdaptor.GetHeight(position.width, new GUIContent("Graphs"))), new GUIContent("Graphs"));
        }

    }
}
