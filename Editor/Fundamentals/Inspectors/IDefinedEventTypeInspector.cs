using System;
using System.Linq;
using Unity.VisualScripting.Community;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace Unity.VisualScripting
{
    [Inspector(typeof(DefinedEventType))]
    public sealed class IDefinedEventTypeInspector : Inspector
    {
        public IDefinedEventTypeInspector(Metadata metadata) : base(metadata) { }

        public override void Initialize()
        {
            base.Initialize();
        }

        private IFuzzyOptionTree GetOptions()
        {
            var alltypes = Codebase.settingsAssembliesTypes
                    .Where(type => typeof(IDefinedEvent).IsAssignableFrom(type) && type != typeof(IDefinedEvent))
                    .ToArray();
            return new TypeOptionTree(alltypes);
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return HeightWithLabel(metadata, width, EditorGUIUtility.singleLineHeight, label);
        }

        public override float GetAdaptiveWidth()
        {
            var definedEventType = metadata.value as DefinedEventType;

            if (definedEventType != null)
            {
                return LudiqGUI.GetTypeFieldAdaptiveWidth(definedEventType.type);
            }

            return base.GetAdaptiveWidth();
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var definedEventType = metadata.value as DefinedEventType;

            position = BeginLabeledBlock(metadata, position, label);

            var fieldPosition = new Rect
                (
                position.x,
                position.y,
                position.width,
                EditorGUIUtility.singleLineHeight
                );

            var newType = LudiqGUI.TypeField(fieldPosition, GUIContent.none, definedEventType.type, GetOptions, new GUIContent("(No Type)"));

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = new DefinedEventType(newType);
            }
        }
    }
}