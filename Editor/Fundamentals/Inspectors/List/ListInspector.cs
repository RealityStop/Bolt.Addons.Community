using UnityEngine;
using System.Collections;

namespace Unity.VisualScripting.Community
{
#if NEW_LIST_UI
    [Inspector(typeof(IList))]
#endif
    public class ListInspector : VisualScripting.ListInspector
    {
        public new ListAdaptor adaptor { get; private set; }

        public ListInspector(Metadata metadata) : base(metadata)
        {
            adaptor = new ListAdaptor(metadata, this);
        }

        protected override float GetHeight(float width, GUIContent label)
        {
            if (metadata.ToString().EndsWith("typeOptions"))
            {
                return base.GetHeight(width, label);
            }
            return adaptor.GetHeight(width, label);
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (metadata.ToString().EndsWith("typeOptions"))
            {
                base.OnGUI(position, label);
                return;
            }

            var normal = GUI.backgroundColor;
            adaptor.Field(position, label);
            // Restore color after tinting add
            GUI.backgroundColor = normal;
        }

        public override float GetAdaptiveWidth() => adaptor.GetAdaptiveWidth();
    }
}
