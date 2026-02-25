using System.Collections;
using UnityEngine;

namespace Unity.VisualScripting.Community
{
#if NEW_DICTIONARY_UI
    [Inspector(typeof(IDictionary))]
#endif
    public class DictionaryInspector : Inspector
    {
        public DictionaryInspector(Metadata metadata) : base(metadata)
        {
            adaptor = new DictionaryAdaptor(metadata, this);
        }

        protected DictionaryAdaptor adaptor { get; private set; }

        protected override float GetHeight(float width, GUIContent label)
        {
            return adaptor.GetHeight(width, label);
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            var normal = GUI.backgroundColor;
            adaptor.Field(position, label);
            // Restore color after tinting add
            GUI.backgroundColor = normal;
        }

        public override float GetAdaptiveWidth()
        {
            return adaptor.GetAdaptiveWidth();
        }
    }
}
