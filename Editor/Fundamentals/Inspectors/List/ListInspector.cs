using UnityEngine;
using System.Collections;
using System;

namespace Unity.VisualScripting.Community
{
#if NEW_LIST_UI
    [Inspector(typeof(IList))]
#endif
    public class ListInspector : Inspector
    {
        public ListAdaptor adaptor { get; private set; }
        public MetadataListAdaptor metadataListAdaptor { get; private set; }

        public ListInspector(Metadata metadata) : base(metadata)
        {
            if (metadata.ToString().EndsWith("typeOptions"))
                metadataListAdaptor = new MetadataListAdaptor(metadata, this);
            else
                adaptor = new ListAdaptor(metadata, this);

        }

        protected override float GetHeight(float width, GUIContent label)
        {
            if (metadata.ToString().EndsWith("typeOptions"))
            {
                return metadataListAdaptor.GetHeight(width, label);
            }
            return adaptor.GetHeight(width, label);
        }

        protected override void OnGUI(Rect position, GUIContent label)
        {
            if (metadata.ToString().EndsWith("typeOptions"))
            {
                metadataListAdaptor.Field(position, label);
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
