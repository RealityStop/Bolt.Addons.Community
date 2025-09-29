using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
    [Inspector(typeof(DefinedEventType))]
    public sealed class DefinedEventTypeInspector : Inspector
    {
        private readonly TypeFilter _typeFilter;

        static Type[] Types;

        public DefinedEventTypeInspector(Metadata metadata) : base(metadata)
        {
            if (metadata.HasAttribute<TypeFilter>())
            {
                _typeFilter = metadata.GetAttribute<TypeFilter>();
            }

            Types ??= Codebase.runtimeTypes.Where(t => !NameUtility.TypeHasSpecialName(t)).ToArray();
        }

        Type[] allTypes;
        private IFuzzyOptionTree GetOptions()
        {
            if (allTypes == null)
            {
                if (!CommunityOptionFetcher.DefinedEvent_RestrictEventTypes)
                {
                    allTypes = Types;
                }
                else if (_typeFilter != null)
                {
                    allTypes = Types.Where(t => _typeFilter.Types.Any(type => type.IsAssignableFrom(t)) && t != typeof(IDefinedEvent))
                        .ToArray();
                }
                else
                {
                    allTypes = Types
                        .Where(t => typeof(IDefinedEvent).IsAssignableFrom(t) && t != typeof(IDefinedEvent))
                        .ToArray();
                }
            }

            return new TypeOptionTree(allTypes);
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            return HeightWithLabel(metadata, width, EditorGUIUtility.singleLineHeight, label);
        }

        public override float GetAdaptiveWidth()
        {
            if (metadata.value is DefinedEventType definedEventType)
            {
                return LudiqGUI.GetTypeFieldAdaptiveWidth(definedEventType.type);
            }

            return base.GetAdaptiveWidth();
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var definedEventType = metadata.value as DefinedEventType;

            position = BeginLabeledBlock(metadata, position, label);

            var fieldPosition = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);

            var newType = LudiqGUI.TypeField(fieldPosition, GUIContent.none, definedEventType.type, GetOptions, new GUIContent("(No Type)"));

            if (EndBlock(metadata))
            {
                metadata.RecordUndo();
                metadata.value = new DefinedEventType(newType);
            }
        }
    }
}
