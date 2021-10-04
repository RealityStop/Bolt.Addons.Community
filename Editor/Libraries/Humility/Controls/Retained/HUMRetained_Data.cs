using UnityEngine.UIElements;

namespace Unity.VisualScripting.Community.Libraries.Humility
{
    public static partial class HUMRetained
    {
        public static partial class Data
        {
            public struct Set
            {
                public VisualElement element;

                public Set(VisualElement element)
                {
                    this.element = element;
                }
            }

            public struct SetMargin
            {
                public Set set;

                public SetMargin(Set set)
                {
                    this.set = set;
                }
            }

            public struct SetPadding
            {
                public Set set;

                public SetPadding(Set set)
                {
                    this.set = set;
                }
            }

            public struct SetBorder
            {
                public Set set;

                public SetBorder(Set set)
                {
                    this.set = set;
                }
            }
        }
    }
}
