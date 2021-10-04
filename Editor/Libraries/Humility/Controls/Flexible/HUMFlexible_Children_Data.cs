using UnityEditor;
using UnityEngine;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMEditor_Flexible_Children
    {
        public static partial class Data
        {
            public struct Selection
            {
                public HUMEditor.Data.Flexible immediate;

                public Selection(HUMEditor.Data.Flexible immediate)
                {
                    this.immediate = immediate;
                }
            }

            public struct Image
            {
                public HUMEditor.Data.Flexible immediate;

                public Image(HUMEditor.Data.Flexible immediate)
                {
                    this.immediate = immediate;
                }
            }

            public struct Button
            {
                public Image image;

                public Button(Image image)
                {
                    this.image = image;
                }
            }

            public struct Toggle
            {
                public Image image;

                public Toggle(Image image)
                {
                    this.image = image;
                }
            }
        }
    }
}
